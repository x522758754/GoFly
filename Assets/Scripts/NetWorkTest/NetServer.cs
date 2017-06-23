using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using UnityEngine;

namespace NetWork
{
    public class NetServer
    {
        public static readonly NetServer Instance = new NetServer();
        private const int maxClient = 10;
        private const int port = 5555;

        private Socket server;
        private Stack<NetUserToken> pools;
        private List<NetUserToken> utList = new List<NetUserToken>();

        private NetServer()
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public void Start()
        {
            server.Listen(maxClient);

            print("Server OK!");

            pools = new Stack<NetUserToken>();
            for(int i=0; i < maxClient; ++i)
            {
                NetUserToken ut = new NetUserToken();
                pools.Push(ut);
            }

            server.BeginAccept(AsyncAceept, null);
        }

        private void AsyncAceept(IAsyncResult rs)
        {
            try
            {
                Socket client = server.EndAccept(rs);

                NetUserToken ut = pools.Pop();
                ut.socket = client;
                BeginReceive(ut);

                utList.Add(ut);

                server.BeginAccept(AsyncAceept, null);
            }
            catch(Exception e)
            {
                print(e.Message);
            }
        }

        private void BeginReceive(NetUserToken ut)
        {
            try
            {
                ut.socket.BeginReceive(ut.buffer, 0, ut.buffer.Length, SocketFlags.None, EndReceive, ut);
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void EndReceive(IAsyncResult rs)
        {
            try
            {
                NetUserToken ut = rs.AsyncState as NetUserToken;
                int len = ut.socket.EndReceive(rs);
                if(len > 0)
                {
                    byte[] data = new byte[len];
                    Buffer.BlockCopy(ut.buffer, 0, data, 0, len);
                    ut.Receive(data);
                    BeginReceive(ut);
                }
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

        public void Send(PBCodeEnum code)
        {
            for(int i=0; i != utList.Count; ++i)
            {
                var ut = utList[i];
                ut.HandleMsg(0, (uint)code, null);
            }
        }
    }
}
