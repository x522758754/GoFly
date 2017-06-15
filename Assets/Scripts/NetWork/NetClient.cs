using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace NetWork
{
    public class NetClient
    {
        TcpClient tc = new TcpClient();
        IPEndPoint ip;
        public byte[] buffer;
        private List<byte> receiveCache = new List<byte>();
        private bool isReceiving;


        public void Connent()
        {
            try
            {
                if(!tc.Connected)
                {
                    buffer = new byte[1024];

                    ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);

                    tc.Connect(ip);

                    BeginReceive();
                }
                else
                {
                    print("Connected!");
                }

            }
            catch (Exception e)
            {
                print(e);
            }
        }

        public void Send(string msg)
        {
            //byte[] result = Encoding.UTF8.GetBytes(msg);
            //Encoding.Default.GetBytes(msg)
            //Buffer
            //Arrary
            //BitConvert
            //MemoryStream
            //StreamReader
            //StreamWriter
            //BinaryReader
            //BinaryWriter

            //数据信息
            NetModel model = new NetModel() { ID = 1, Commit = "LanOu", Message = msg };
            byte[] dataModel = NetModel.Serialize(model);

            //长度信息
            int len = dataModel.Length;
            byte[] dataLength = BitConverter.GetBytes(len);

            //合并数组
            byte[] result = NetEncode.Encode(dataModel);

            tc.GetStream().Write(result, 0, result.Length);

        }

        private void BeginReceive()
        {
            try
            {
                tc.GetStream().BeginRead(buffer, 0, buffer.Length, EndReceive, tc);
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public void EndReceive(IAsyncResult rs)
        {
            try
            {
                TcpClient t = rs.AsyncState as TcpClient;
                int len = t.Client.EndReceive(rs);
                if (len > 0)
                {
                    byte[] data = new byte[len];
                    Buffer.BlockCopy(buffer, 0, data, 0, len);

                    Receive(data);
                    BeginReceive();
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public void Receive(byte[] data)
        {
            print("receive data.");
            receiveCache.AddRange(data);

            if (!isReceiving)
            {
                isReceiving = true;
                ReadData();
            }
        }

        private void ReadData()
        {
            byte[] data = NetEncode.Decode(ref receiveCache);
            if (null != data)
            {
                NetModel item = NetModel.Deserialize(data);
                print("cleint:"+ item.Message);

                ReadData();
            }
            else
            {
                isReceiving = false;
            }
        }

        private void print(object o)
        {
            Util.LoggerHelper.Log(o);

        }
    }
}
