using System;
using System.Collections.Generic;

namespace NetWork
{
    public class Packet
    {
        public int nCode;
        public object objBody;

        public Packet(int code, object msg)
        {
            nCode = code;
            objBody = msg;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", nCode, objBody);
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
        public Action m_callback;

        public SendPacket(int code, object buffer):base(code, buffer)
        {

        }
    }
}
