/*  客户端Tcp
 * 
 * */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Util;

namespace NetWork
{
    public class TcpClientWorker
    {
        private const int c_MaxBufferSize = 65535;
        private const int c_ThreadSleepTime = 100;

        private Socket m_socket;
        private byte[] m_recvBuffer;
        private byte[] m_sendBuffer;
        private Queue<Packet> m_sendQueue = new Queue<Packet>();
        private Queue<Packet> m_recvQueue = new Queue<Packet>();
        private int m_recvUnreadBytes = 0;//recvBuffer缓存但没被读读取的字节数

        /// <summary>
        /// 线程运行标记
        /// </summary>
        private bool m_threadRunFlag = true;
        /// <summary>
        /// 异步发送标记
        /// </summary>
        private bool m_asyncSend = true;
        /// <summary>
        /// 网络通信同步锁
        /// </summary>
        private readonly object m_tcpClientLocker = new object();
        /// <summary>
        /// 接收数据队列同步锁。
        /// </summary>
        private readonly object m_recvQueueLocker = new object();
        /// <summary>
        /// 发送数据队列同步锁。
        /// </summary>
        private readonly object m_sendQueueLocker = new object();
        /// <summary>
        /// 异步读取数据线程。
        /// </summary>
        private Thread m_receiveThread;
        /// <summary>
        /// 异步发送数据线程。
        /// </summary>
        private Thread m_sendThread;


        public TcpClientWorker()
        {
            m_recvBuffer = new byte[c_MaxBufferSize];
            m_sendBuffer = new byte[c_MaxBufferSize];
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            lock(m_tcpClientLocker)
            {
                if ((null != m_socket) && (true == m_socket.Connected))
                {
                    throw new Exception("Exception. the tcpClient has Connectted.");
                }

                try
                {
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_socket.NoDelay = true;
                    m_socket.Connect(ip, port);

                    if(null != m_socket)
                    {
                        if(null == m_receiveThread)
                        {
                            m_receiveThread = new Thread(new ThreadStart(DoReceive));
                            m_receiveThread.IsBackground = true;
                        }

                        if (!m_receiveThread.IsAlive)
                            m_receiveThread.Start();

                        if(null == m_sendThread)
                        {
                            m_sendThread = new Thread(new ThreadStart(AsyncSend));
                            m_sendThread.IsBackground = true;
                        }

                        if (!m_sendThread.IsAlive)
                            m_sendThread.Start();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Exception. the tcpClient do connecting error:{0}", e));
                }
                LoggerHelper.Log("Connect Ok!");
            }
        }

        /// <summary>
        /// 供外部调用发送网络消息
        /// </summary>
        /// <param name="p"></param>
        public void Send(Packet p)
        {
            if(null == m_socket || (false == m_socket.Connected))
            {
                return;
            }
            lock (m_sendQueueLocker)
                m_sendQueue.Enqueue(p);
        }

        /// <summary>
        ///供外部调用接收网络消息
        /// </summary>
        /// <returns></returns>
        public Packet Recv()
        {
            Packet packet = null;
            if (m_recvQueue.Count > 0)
            {
                lock (m_recvQueueLocker)
                    packet = m_recvQueue.Dequeue();
            }

            return packet;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            Clear();

            m_asyncSend = false;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Release()
        {
            Clear();

            //线程不用主动中断，只要保证内部没有执行，清除引用后会自动回收
            m_threadRunFlag = false;
            //m_receiveThread.Abort();
            m_receiveThread = null;
            //m_sendThread.Abort();
            m_sendThread = null;

            m_recvBuffer = null;
            m_sendBuffer = null;
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        private void AsyncSend()
        {
            while (m_threadRunFlag)
            {
                if (true == m_asyncSend)
                {
                    DoSend();
                    Thread.Sleep(c_ThreadSleepTime);
                }
            }
        }

        /// <summary>
        /// 处理发送
        /// </summary>
        private void DoSend()
        {
            lock (m_tcpClientLocker)
            {
                if ((m_socket == null) || (m_socket.Connected == false))
                {
                    return;
                }
            }

            int nTotalLength = 0;

            //并包
            lock (m_sendQueueLocker)
            {
                while (nTotalLength < c_MaxBufferSize && m_sendQueue.Count > 0)
                {
                    Packet packet = m_sendQueue.Peek();
                    byte[] bytes = NetEnCoder.Encode(packet);


                    if (nTotalLength + bytes.Length < c_MaxBufferSize)
                    {
                        Buffer.BlockCopy(bytes, 0, m_sendBuffer, nTotalLength, bytes.Length);
                        nTotalLength += bytes.Length;
                        m_sendQueue.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            try
            {
                if (nTotalLength > 0)
                {
                    m_socket.Send(m_sendBuffer, 0, nTotalLength, SocketFlags.None);
                    Array.Clear(m_sendBuffer, 0, c_MaxBufferSize);
                    nTotalLength = 0;
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
            }
        }

        /// <summary>
        /// 处理网络接收
        /// </summary>
        private void DoReceive()
        {
            int byteRead = 0;//网络流可读取的字节数
            int recvBufferFree = 0;//recvBuffer可用的字节数

            while (m_threadRunFlag)
            {
                try
                {
                    recvBufferFree = c_MaxBufferSize - m_recvUnreadBytes;
                    if (recvBufferFree > 0)
                    {
                        byteRead = m_socket.Receive(m_recvBuffer, m_recvUnreadBytes, recvBufferFree, SocketFlags.None);
                        m_recvUnreadBytes += byteRead;
                    }
                    else
                    {
                        //缓存不够时继续循环，后面会对缓存数据进行处理
                    }
                }
                catch (ObjectDisposedException e)
                {
                    // 网络流已被关闭，结束接收数据
                    LoggerHelper.Error(string.Format("tcp close{0}", e)); ;
                }
                catch (System.IO.IOException e)
                {
                    //捕获WSACancelBlockingCall()导致的异常。
                    //原因：强迫终止一个在进行的阻塞调用。
                    //可直接捕获忽略，应该不会有不良影响。
                    LoggerHelper.Except(e);
                }
                catch (Exception e)
                {
                    LoggerHelper.Except(e);
                }

                SplitPackets();                
            }
        }

        /// <summary>
        /// 切包
        /// </summary>
        private void SplitPackets()
        {
            try
            {
                int offset = 0; //recvBuffer的整体读取游标
                while (m_recvUnreadBytes > NetEnCoder.GetIntLength())
                {
                    try
                    {
                        //备注：消息协议=长度（nCode和body所占字节长度） + sessionId +  nCode + body
                        int nLength = NetEnCoder.DecodeInt(m_recvBuffer, ref offset);//nocde + body 所占字节数
                        m_recvUnreadBytes -= offset;

                        if (m_recvUnreadBytes >= nLength)
                        {

                            uint uSession = NetEnCoder.DecodeUInt(m_recvBuffer, ref offset);

                            uint uCode = NetEnCoder.DecodeUInt(m_recvBuffer, ref offset);

                            int nCount = nLength - 2 * NetEnCoder.GetIntLength();
                            object msg = PBEnCoder.Decode(uCode, m_recvBuffer, offset, nCount);

                            Packet packet = new Packet(uSession, uCode, msg);
                            lock(m_recvQueueLocker)
                            {
                                LoggerHelper.Log(packet.ToString());
                                m_recvQueue.Enqueue(packet);
                            }

                            offset += nCount;
                            m_recvUnreadBytes -= nLength;
                        }
                        else
                        {
                            m_recvUnreadBytes += offset;
                            offset -= NetEnCoder.GetIntLength();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        LoggerHelper.Except(e);
                        break;
                    }

                }

                // 整理 RecvBuffer， 将buffer 内容前移
                Buffer.BlockCopy(m_recvBuffer, offset, m_recvBuffer, 0, m_recvUnreadBytes);
            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
                LoggerHelper.Critical("SplitPackets error.");
                Close();
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        private void Clear()
        {
            if(null != m_socket)
            {
                if(true == m_socket.Connected)
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                    m_socket.Close();
                }

                m_socket = null;
            }

            if(null != m_recvBuffer)
            {
                Array.Clear(m_recvBuffer, 0, c_MaxBufferSize);
            }
            
            if(null != m_sendBuffer)
            {
                Array.Clear(m_sendBuffer, 0, c_MaxBufferSize);
            }
        }
    }
}
