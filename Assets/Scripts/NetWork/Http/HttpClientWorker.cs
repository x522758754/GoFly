/* Http 客户端
 * 
 * */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using Util;

namespace NetWork
{
    public class HttpClientWorker
    {
        public const int c_MaxBufferSize = 65535;
        public const int c_ThreadSleepTime = 100;
        public const string c_httpMethod = "POST";

        private byte[] m_recvBuffer;
        private byte[] m_sendBuffer;
        private Queue<HttpPacket> m_sendQueue = new Queue<HttpPacket>();
        private Queue<HttpPacket> m_recvQueue = new Queue<HttpPacket>();
        /// <summary>
        /// 接收数据队列同步锁。
        /// </summary>
        private readonly object m_recvQueueLocker = new object();
        /// <summary>
        /// 发送数据队列同步锁。
        /// </summary>
        private readonly object m_sendQueueLocker = new object();
        /// <summary>
        /// 工作线程
        /// </summary>
        private Thread m_thread;
        /// <summary>
        /// 线程运行标记
        /// </summary>
        private bool m_threadRunFlag = true;
        /// <summary>
        /// 网络运行标志
        /// </summary>
        private bool m_netRunFlag = true;

        private WebClient m_webClient = new WebClient();

        public HttpClientWorker()
        {
            m_recvBuffer = new byte[c_MaxBufferSize];
            m_sendBuffer = new byte[c_MaxBufferSize];

            m_thread = new Thread(WebThreadHandle);
            m_thread.IsBackground = true;
            if (!m_thread.IsAlive)
                m_thread.Start();

        }

        /// <summary>
        /// 处理网络操作线程
        /// </summary>
        private void WebThreadHandle()
        {
            while(m_threadRunFlag)
            {
                if(!m_netRunFlag)
                {
                    continue;
                }

                HttpPacket sendPacket = null;
                lock (m_sendQueue)
                {
                    if (m_sendQueue.Count > 0)
                    {
                        sendPacket = m_sendQueue.Dequeue();
                    }
                }

                if (null == sendPacket)
                {
                    Thread.Sleep(c_ThreadSleepTime);
                    continue;
                }

                try
                {
                    byte[] bytes = NetEnCoder.Encode(sendPacket);
                    byte[] byteRes = m_webClient.UploadData(NetHttpManager.Instance.GetUrl(), c_httpMethod, bytes);

                    if(null != byteRes)
                    {
                        //解析http返回包
                        //目前解析 太浪费内存、使用缓冲区
                        int resLength = byteRes.Length;
                        int offset = 0;
                        int nLength = NetEnCoder.DecodeInt(byteRes, ref offset);
                        resLength -= offset;

                        if (resLength >= nLength)
                        {
                            uint uCode = NetEnCoder.DecodeUInt(byteRes, ref offset);

                            int nCount = nLength - NetEnCoder.GetIntLength();
                            object msg = PBEnCoder.Decode(uCode, byteRes, offset, nCount);
                            HttpPacket recvPacket = new HttpPacket(uCode, nCount);
                            recvPacket.handler = sendPacket.handler;

                            lock (m_recvQueueLocker)
                            {
                                m_recvQueue.Enqueue(recvPacket);
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    LoggerHelper.Except(e);
                }


            }
        }

        /// <summary>
        /// 发送http请求
        /// </summary>
        /// <param name="p"></param>
        public void SendHttp(HttpPacket p)
        {
            lock(m_sendQueue)
            {
                m_sendQueue.Enqueue(p);
            }
        }

        public void Close()
        {
            m_netRunFlag = false;
        }

    }
}
