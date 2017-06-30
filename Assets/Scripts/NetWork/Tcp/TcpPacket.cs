using System;
using System.Collections.Generic;
using Util;

namespace NetWork
{
    public class TcpPacket : Packet
    {
        /// <summary>
        /// 会话id
        /// </summary>
        public uint uSession;

        /// <summary>
        /// 用户自定数据
        /// </summary>
        public object userCustomData;

        /// <summary>
        /// 收到服务器消息回调
        /// </summary>
        public NetSessionHandler netTcpHandler;

        public uint timerId;

        public TcpPacket(uint session, uint code, object buffer) : base(code, buffer)
        {
            uSession = session;
        }

        /// <summary>
        /// 接收超时处理
        /// </summary>
        public void OnRecvOverTime()
        {
            uint overTime = NetTcpManager.Instance.RecvOverTime();
            timerId = TimerHeap.AddTimer(overTime, 0, () =>
            {
                if (null != netTcpHandler)
                {
                    netTcpHandler(false, null, null);
                    NetTcpManager.Instance.RemoveSession(uSession);
                }

            });
        }
    }
}
