/* 
 * 
 * */
using System;
using System.Collections.Generic;
using Util;

namespace NetWork
{
    /// <summary>
    /// 会话回调
    /// </summary>
    /// <param name="isSuccess">是否成功</param>
    /// <param name="p">回传信息</param>
    /// <param name="customData">用户自定义信息</param>
    public delegate void NetSessionHandler(bool isSuccess, Packet p, object customData);
    /// <summary>
    /// 推送通知
    /// </summary>
    /// <param name="p"></param>
    public delegate void NetPushHandler(Packet p);

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
        
        /// <summary>
        /// 网络超时时间
        /// </summary>
        public uint OverTime;
        /// <summary>
        /// tcp客户端
        /// </summary>
        private TcpClientWorker m_tcpClient;
        /// <summary>
        /// 下一个会话id
        /// </summary>
        private uint m_nNextSessionId;
        /// <summary>
        /// 请求会话表(session, SendPacket)
        /// </summary>
        private Dictionary<uint, TcpPacket> m_sessions;
        /// <summary>
        /// 推送表(code, Packet)
        /// </summary>
        private Dictionary<uint, NetPushHandler> m_pushHandlers;

        public void Init()
        {
            m_sessions = new Dictionary<uint, TcpPacket>();
            m_pushHandlers = new Dictionary<uint, NetPushHandler>();

            m_tcpClient = new TcpClientWorker();
            string ip = "127.0.0.1"; //端口和ip应该有公共方法进行读取
            int port = 5555;
            LoggerHelper.Log(string.Format("ip: {0}, port: {1}", ip, port));
            m_tcpClient.Connect(ip, port);
        }

        public void Release()
        {
            if (null != m_tcpClient)
            {
                m_tcpClient.Release();
                m_tcpClient = null;
            }

            ResetSession();
        }

        public void Send(uint uCode, object pbMsg, NetSessionHandler handler, object customData)
        {
            TcpPacket sendPacket = new TcpPacket(AllocatSessionId(), uCode, pbMsg);

            sendPacket.userCustomData = customData;
            sendPacket.netTcpHandler = handler;
            sendPacket.OnRecvOverTime();

            m_sessions.Add(sendPacket.uSession, sendPacket);

            m_tcpClient.Send(sendPacket);
        }

        /// <summary>
        /// 供系统帧调用
        /// </summary>
        public void Recv()
        {
            if (null == m_tcpClient)
                return;
            TcpPacket packet = m_tcpClient.Recv();
            if (null != packet)
            {
                uint uSession = packet.uSession;
                if (m_sessions.ContainsKey(uSession))
                {
                    var sp = m_sessions[uSession];
                    if (null != sp.netTcpHandler)
                    {
                        sp.netTcpHandler(true, packet, sp.userCustomData);
                    }

                    RemoveSession(uSession);
                }
                else if(m_pushHandlers.ContainsKey(packet.uCode))
                {
                    if (null != m_pushHandlers[packet.uCode])
                    {
                        m_pushHandlers[packet.uCode](packet);
                    }
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
            return m_sessions.ContainsKey(m_nNextSessionId) ? AllocatSessionId() : m_nNextSessionId;
        }

        /// <summary>
        /// 重置会话
        /// </summary>
        private void ResetSession()
        {
            m_nNextSessionId = 0;

            if(null != m_sessions)
            {
                using (var enu = m_sessions.GetEnumerator())
                {
                    while (enu.MoveNext())
                    {
                        var item = enu.Current.Value;
                        item.netTcpHandler = null;
                        TimerHeap.DelTimer(item.timerId);
                    }
                }
                m_sessions.Clear();
                m_sessions = null;
            }
        }

        /// <summary>
        /// 移除会话
        /// </summary>
        /// <param name="sId"></param>
        public void RemoveSession(uint sId)
        {
            if(m_sessions.ContainsKey(sId))
            {
                var item = m_sessions[sId];
                item.netTcpHandler = null;
                TimerHeap.DelTimer(item.timerId);
                m_sessions.Remove(sId);
            }
        }

        /// <summary>
        /// 获取超时时间
        /// </summary>
        /// <returns></returns>
        public uint RecvOverTime()
        {
            if (0 == OverTime)
            {
                //读取配置、暂时设置10s
                return 10 * 1000;
            }

            return OverTime;
        }

        /// <summary>
        /// 注册推送事件
        /// </summary>
        /// <param name="code"></param>
        /// <param name="handler"></param>
        public void RegisterPushHandler(uint code, NetPushHandler handler)
        {
            if(m_pushHandlers.ContainsKey(code))
            {
                m_pushHandlers[code] += handler;
            }
            else
            {
                NetPushHandler callback = null;
                m_pushHandlers.Add(code, callback);
                m_pushHandlers[code] += handler;
            }
        }

        /// <summary>
        /// 移除推送事件
        /// </summary>
        /// <param name="code"></param>
        /// <param name="handler"></param>
        public void RemovePushHandler(uint code, NetPushHandler handler)
        {
            if(m_pushHandlers.ContainsKey(code))
            {
                m_pushHandlers[code] -= handler;
            }
        }
    }
}
