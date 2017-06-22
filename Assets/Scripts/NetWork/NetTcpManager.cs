/* 
 * 
 * */
using System;
using System.Collections.Generic;
using Util;

namespace NetWork
{
    public delegate void NetTcpHandler(bool isSuccess, Packet p, object customData);
    public class NetTcpManager:Singleton<NetTcpManager>
    {
        #region 数据包头
        /*
         * 客户端时间
         * 服务器ID
         * 玩家Id
         * 登录凭证
         * */
        #endregion

        #region 网络状态
        /*
         * NONE
         * CREATE
         * CONNECTING
         * WORKING
         * CLOSE
         * ERROR
         * */
        #endregion

        private TcpClientWorker m_client;
        private uint m_nNextSessionId;
        private Dictionary<uint, SendPacket> m_sendPackets;

        public void Init()
        {
            m_sendPackets = new Dictionary<uint, SendPacket>();
            m_client = new TcpClientWorker();
            string ip = "127.0.0.1"; //端口和ip应该有公共方法进行读取
            int port = 5555;
            LoggerHelper.Log(string.Format("ip: {0}, port: {1}", ip, port));
            m_client.Connect(ip, port);
        }

        public void Release()
        {
            if (null != m_client)
            {
                m_client.Release();
                m_client = null;
            }

            ResetSession();
        }

        public void Send(int nCode, object pbMsg, NetTcpHandler handler, object customData)
        {
            SendPacket sendPacket = new SendPacket(AllocatSessionId(), nCode, pbMsg);

            sendPacket.userCustomData = customData;
            sendPacket.netTcpHandler = handler;
            sendPacket.OnRecvOverTime();

            m_sendPackets.Add(sendPacket.uSession, sendPacket);

            m_client.Send(sendPacket);
        }

        public void Recv()
        {
            if (null == m_client)
                return;
            Packet packet = m_client.Recv();
            if (null != packet)
            {
                uint uSession = packet.uSession;
                if (m_sendPackets.ContainsKey(uSession))
                {
                    var sp = m_sendPackets[uSession];
                    if (null != sp.netTcpHandler)
                    {
                        sp.netTcpHandler(true, packet, sp.userCustomData);
                    }

                    RemoveSession(uSession);
                }
}
        }

        /// <summary>
        /// 分配会话Id
        /// </summary>
        private uint AllocatSessionId()
        {
            if(m_nNextSessionId > uint.MaxValue)
            {
                m_nNextSessionId = 0;
            }
            m_nNextSessionId++;

            //尾递归
            return m_sendPackets.ContainsKey(m_nNextSessionId) ? AllocatSessionId() : m_nNextSessionId;
        }

        /// <summary>
        /// 重置会话
        /// </summary>
        private void ResetSession()
        {
            m_nNextSessionId = 0;

            using (var enu = m_sendPackets.GetEnumerator())
            {
                while(enu.MoveNext())
                {
                    var item = enu.Current.Value;
                    item.netTcpHandler = null;
                    TimerHeap.DelTimer(item.timerId);
                }
            }
            m_sendPackets.Clear();
            m_sendPackets = null;
        }

        /// <summary>
        /// 移除会话
        /// </summary>
        /// <param name="sId"></param>
        public void RemoveSession(uint sId)
        {
            if(m_sendPackets.ContainsKey(sId))
            {
                var item = m_sendPackets[sId];
                item.netTcpHandler = null;
                TimerHeap.DelTimer(item.timerId);
                m_sendPackets.Remove(sId);
            }
        }

        /// <summary>
        /// 获取超时时间
        /// </summary>
        /// <returns></returns>
        public uint RecvOverTime()
        {
            //可以配置
            return 10 * 1000;
        }
    }
}
