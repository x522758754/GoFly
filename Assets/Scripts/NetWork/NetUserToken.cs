using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace NetWork
{
    public class NetUserToken
    {
        private const int size = 1024;

        public Socket socket;
        public byte[] buffer;
        private List<byte> receiveCache;
        private bool isReceiving;
        private Queue<byte[]> sendCache;
        private bool isSending;

        public Action<NetModel> receiveCallback;

        public NetUserToken()
        {
            buffer = new byte[1024];
            receiveCache = new List<byte>();
            sendCache = new Queue<byte[]>();
        }

        public void Receive(byte[] data)
        {
            print("receive data.");
            receiveCache.AddRange(data);

            if(!isReceiving)
            {
                isReceiving = true;
                ReadData();
            }
        }

        private void ReadData()
        {
            byte[] data = NetEncode.Decode(ref receiveCache);
            if(null != data)
            {
                NetModel item = NetModel.Deserialize(data);
                print(item.Message);
                if(null != receiveCallback)
                {
                    receiveCallback(item);
                }

                ReadData();

                WriteSendData(NetEncode.Encode(data));
            }
            else
            {
                isReceiving = false;
            }
        }

        public void Send()
        {
            try
            {
                if(0 == sendCache.Count)
                {
                    isSending = false;
                    return;
                }

                byte[] data = sendCache.Dequeue();
                int count = data.Length / size;
                int len = size;
                for(int i=0; i < count + 1; ++i)
                {
                    if(i == count)
                    {
                        len = data.Length - i * size;
                    }
                    socket.Send(data, i * size, len, SocketFlags.None);
                }

                print("发送成功");
                Send();
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void print(object o)
        {
            Util.LoggerHelper.Log(o);
        }

        public void WriteSendData(byte[] data)
        {
            sendCache.Enqueue(data);
            if(!isSending)
            {
                isSending = true;
                Send();
            }
        }
    }
}
