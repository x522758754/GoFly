using System;
using System.Collections.Generic;
using Util;

namespace NetWork
{
    public class Packet
    {
        /// <summary>
        /// 会话id
        /// </summary>
        public uint uSession;

        /// <summary>
        /// 协议id
        /// </summary>
        public int nCode;

        /// <summary>
        /// 协议体
        /// </summary>
        public object msgBody;

        public Packet(uint session, int code, object msg)
        {
            uSession = session;
            nCode = code;
            msgBody = msg;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", nCode, msgBody);
        }

    }

    public class SendPacket: Packet
    {
        /// <summary>
        /// 用户自定数据
        /// </summary>
        public object userCustomData;

        /// <summary>
        /// 收到服务器消息回调
        /// </summary>
        public NetTcpHandler netTcpHandler;

        public uint timerId;

        public SendPacket(uint session, int code, object buffer):base(session, code, buffer)
        {
        }

        /// <summary>
        /// 接收超时
        /// </summary>
        public void OnRecvOverTime()
        {
            uint overTime = NetTcpManager.Instance.RecvOverTime();
            timerId = TimerHeap.AddTimer(overTime, 0, () =>
            {
                if(null != netTcpHandler)
                {
                    netTcpHandler(false, null, null);
                    NetTcpManager.Instance.RemoveSession(uSession);
                }
                
            });
        }
    }
}
