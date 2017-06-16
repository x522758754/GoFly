/* 网络传输译码器 auto-generate
 * 
 * */

using System;
using System.Collections.Generic;
using System.IO;

namespace NetWork
{
    public partial class NetEnCoder
    {
        public static object Decode(int nCode, byte[] bytes, int offset, int count)
        {
            object result = null;
            using (MemoryStream m = new MemoryStream(bytes, offset, count))
            {
                switch(nCode)
                {
                    //case OpDefine.ErrorMessage:
                    //    result = ProtoBuf.Serializer.Deserialize<ErrorMessage>(m);
                    //    break;
                    //case OpDefine.QueueUpMessage:
                    //    result = ProtoBuf.Serializer.Deserialize<QueueUpMessage>(m);
                    //    break;
                    default:break;
                }
            }

            return result;
        }
    }
}
