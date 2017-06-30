/* Http连接管理类
 * 
 * */
using System;
using System.Collections.Generic;
using Util;

namespace NetWork
{
    /// <summary>
    /// Http消息回调
    /// </summary>
    public delegate void NetHttpHandler(bool isSuccess, HttpPacket p);
    public class NetHttpManager:Singleton<NetHttpManager>
    {
        public HttpClientWorker m_client;

        public void Init()
        {
            m_client = new HttpClientWorker();
        }

        public void Release()
        {

        }

        public void Send()
        {

        }

        public void Recv()
        {

        }
    }
}
