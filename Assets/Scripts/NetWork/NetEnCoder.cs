/* 网络传输译码器
 * 
 * */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using ProtoBuf;
using Util;

namespace NetWork
{
    //注意可能字节端的大小不一样,需要转换
    //如java采用 big endian    C#采用Little endian

    public partial class NetEnCoder
    {
        /// <summary>
        /// Tcppacket -> bytes
        /// </summary>
        public static byte[] Encode(TcpPacket packet)
        {
            int len = 0;

            byte[] sessionBytes = GetBytes(packet.uSession);  //不要直接用BinaryWriter.write(int) 可能端的大小不一样
            len += sessionBytes.Length;

            byte[] codeBytes = GetBytes(packet.uCode);
            len += codeBytes.Length;

            byte[] bodyBytes = GetBytes(packet.msgBody);
            len += bodyBytes.Length;

            byte[] lengthBytes = GetBytes(len);

            byte[] result = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter br = new BinaryWriter(ms);

                ///备注：发送内容=长度（uSession+Code+body共占字节长度） + sessionId + nCode + body
                br.Write(lengthBytes);
                br.Write(sessionBytes);
                br.Write(codeBytes);
                br.Write(bodyBytes);

                //result = new byte[codeBytes.Length + bodyBytes.Length + lengthBytes.Length];
                //Buffer.BlockCopy(ms.ToArray(), 0, result, 0, (int)ms.Length);
                //br.Close();
                //ms.Close();

                result = ms.ToArray();
            }


            return result;
        }

        public static byte[] Encode(HttpPacket packet)
        {
            int len = 0;

            byte[] codeBytes = GetBytes(packet.uCode);
            len += codeBytes.Length;

            byte[] bodyBytes = GetBytes(packet.msgBody);
            len += bodyBytes.Length;

            byte[] lengthBytes = GetBytes(len);

            byte[] result = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter br = new BinaryWriter(ms);

                ///备注：发送内容=长度（Code和body所占字节长度） + nCode + body
                br.Write(lengthBytes);
                br.Write(codeBytes);
                br.Write(bodyBytes);

                result = ms.ToArray();
            }

            return result;
        }

        public static Packet Decode(byte[] bytes)
        {
            int offset = 0;
            int len = DecodeInt(bytes, ref offset);
            

            Packet p = null;

            return p;
        }

        public static int DecodeInt(byte[] bytes, ref int offset)
        {
            //需要判断bytes的Length
            try
            {
                int value = BitConverter.ToInt32(bytes, offset);
                offset += GetIntLength();

                //注意端的大小
                //value = IPAddress.NetworkToHostOrder(value)

                return value;
            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
                return 0;
            }
        }

        public static uint DecodeUInt(byte[] bytes, ref int offset)
        {
            //需要判断bytes的Length
            try
            {
                uint value = BitConverter.ToUInt32(bytes, offset);
                offset += GetIntLength();

                //注意端的大小
                //value = IPAddress.NetworkToHostOrder(value)

                return value;
            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
                return 0;
            }
        }

        public static byte[] GetBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return bytes;
        }

        public static byte[] GetBytes(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return bytes;
        }

        public static byte[] GetBytes(object obj)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, obj);
                    ms.Position = 0;

                    byte[] bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, bytes.Length);

                    return bytes;
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
                return null;
            }
        }

        public static int GetIntLength()
        {
            //return Marshal.SizeOf(typeof(UInt32));
            return 4;
        }
    }
}
