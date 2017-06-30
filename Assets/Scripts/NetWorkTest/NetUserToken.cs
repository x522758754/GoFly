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
                int offset = 0;
                uint uSession = NetEnCoder.DecodeUInt(data, ref offset);
                uint uCode = NetEnCoder.DecodeUInt(data, ref offset);
                int nCount = data.Length - offset;
                object msg = PBEnCoder.Decode(uCode, data, offset, nCount);

                if(0 != uCode)
                {
                    HandleMsg(uSession, uCode, msg);
                }

                ReadData();
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

        public void HandleMsg(uint uSession, uint code, object msg)
        {
            switch((PBCodeEnum)code)
            {
                case PBCodeEnum.CSHeartBeat:
                    {
                        print((CSHeartBeat)msg);
                        SCHeartBeat heart = new SCHeartBeat();
                        heart.clientTime = CommonHelper._UtcNowMs;
                        heart.serverTime = CommonHelper._UtcNowMs + 1;
                        TcpPacket p = new TcpPacket(uSession, (uint)PBCodeEnum.SCHeartBeat, heart);
                        byte[] bytes = NetEnCoder.Encode(p);
                        WriteSendData(bytes);
                    }
                    break;
                case PBCodeEnum.CSLogin:
                    {
                        print((CSLogin)msg);
                        SCLogin login = new SCLogin();
                        login.errorCode = 0;
                        login.loginRet = 1;
                        TcpPacket p = new TcpPacket(uSession, (uint)PBCodeEnum.SCLogin, login);
                        byte[] bytes = NetEnCoder.Encode(p);
                        WriteSendData(bytes);
                    }
                    break;
            }
        }
    }
}
