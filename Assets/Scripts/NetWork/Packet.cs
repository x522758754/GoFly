using System;
using System.Collections.Generic;
using Util;

namespace NetWork
{
    /// <summary>
    /// 网络传输包基类
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// 协议id
        /// </summary>
        public uint uCode;

        /// <summary>
        /// 协议体
        /// </summary>
        public object msgBody;

        public Packet(uint code, object msg)
        {
            uCode = code;
            msgBody = msg;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", uCode, msgBody);
        }

    }

}
