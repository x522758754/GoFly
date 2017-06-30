/* http网络包
 * 
 * */
using System;
using System.Collections.Generic;

namespace NetWork
{
    public class HttpPacket: Packet
    {
        /// <summary>
        /// 发送地址(ip/url)
        /// </summary>
        //public string url;

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object userCustomData;

        /// <summary>
        /// 服务器收到消息回调
        /// </summary>
        public NetHttpHandler handler;

        public HttpPacket(uint uCode, object objMsg):base(uCode, objMsg)
        {
        }
    }
}
