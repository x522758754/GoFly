using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetWork
{
    public class NetEncode
    {
        public static byte[] Encode(byte[] data)
        {
            byte[] result = new byte[data.Length + 4];

            MemoryStream ms = new MemoryStream();
            BinaryWriter br = new BinaryWriter(ms);

            br.Write(data.Length);
            br.Write(data);
            Buffer.BlockCopy(ms.ToArray(), 0, result, 0, (int)ms.Length);
            br.Close();
            ms.Close();

            return result;
        }

        public static byte[] Decode(ref List<byte> cache)
        {
            if(cache.Count < 4)
            {
                return null;
            }

            MemoryStream ms = new MemoryStream(cache.ToArray());
            BinaryReader br = new BinaryReader(ms);
            int len = br.ReadInt32();
            if(len > ms.Length - ms.Position)
            {
                return null;
            }

            byte[] result = br.ReadBytes(len);
            cache.Clear();

            cache.AddRange(br.ReadBytes((int)(ms.Length - ms.Position)));

            return result;
        }
    }
}
