using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.IO;
using UnityEngine;

namespace NetWork
{
    //添加特性，表示可以被ProtoBuf工具序列化
    [ProtoContract]
    public class NetModel
    {
        [ProtoMember(1)]
        public int ID;

        [ProtoMember(2)]
        public string Commit;

        [ProtoMember(3)]
        public string Message;

        public static byte[] Serialize(NetModel model)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize<NetModel>(ms, model);
                    byte[] result = new byte[ms.Length];

                    ms.Position = 0;
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception e)
            {
                print(e.Message);
                return null;
            }
        }

        public static NetModel Deserialize(byte[] msg)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(msg, 0, msg.Length);
                    ms.Position = 0;
                    NetModel result = Serializer.Deserialize<NetModel>(ms);
                    return result;
                }
            }
            catch (Exception e)
            {
                print(e.Message);
                return null;
            }
        }

        private static void print(object o)
        {
            MonoBehaviour.print(o);
        }
    }


}
