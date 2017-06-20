/* 网络传输译码器 auto-generate
 * 
 * */

using System;
using System.Collections.Generic;
using System.IO;
using Util;

namespace NetWork
{
    public class PBEnCoder
    {
        public static object Decode(int nCode, byte[] bytes, int offset, int count)
        {
            object result = null;
            try
            {
                using (MemoryStream m = new MemoryStream(bytes, offset, count))
                {
                    switch ((PBCodeEnum)nCode)
                    {
                        #region 错误信息 10000
                        case PBCodeEnum.ErrorMessage:
                            result = ProtoBuf.Serializer.Deserialize<ErrorMessage>(m);
                            break;
                        #endregion
                        #region 基本通讯 20000
                        case PBCodeEnum.CSLogin:
                            result = ProtoBuf.Serializer.Deserialize<CSLogin>(m);
                            break;
                        case PBCodeEnum.SCLogin:
                            result = ProtoBuf.Serializer.Deserialize<SCLogin>(m);
                            break;
                        case PBCodeEnum.CSHeartBeat:
                            result = ProtoBuf.Serializer.Deserialize<CSHeartBeat>(m);
                            break;
                        case PBCodeEnum.SCHeartBeat:
                            result = ProtoBuf.Serializer.Deserialize<SCHeartBeat>(m);
                            break;
                        #endregion

                        default:
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
            }


            return result;
        }
    }
}
