using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SocketLib
{
    public class TcpHelper
    {
        private int listenPort;
        private Socket tcpListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private EndPoint tcpLocalEnd = null;
        private List<Socket> _AcceptedSockets = new List<Socket>();
        public List<Socket> AcceptedSockets
        {
            get
            {
                return _AcceptedSockets;
            }
        }

        public event Func<Socket, IPEndPoint, bool> ReceiveEvent;

        public TcpHelper(int port)
        {
            listenPort = port;
            
        }
        
        public void Listen()
        {
            tcpLocalEnd = new IPEndPoint(IPAddress.Any, listenPort);
            tcpListenSock.Bind(tcpLocalEnd);

            tcpListenSock.Listen(10);
            Console.WriteLine(">Start listening on port {0}", listenPort);

            //FetchFromQueue();
            //开辟新线程循环接收数据
            ThreadPool.QueueUserWorkItem(obj =>
            {
                //WatchTcpConnectionWithSingleThread();
                WatchTcpConnectionWithMultiThread();
                //WatchTcpConnectionWithSelect();
                //WatchTcpConnectionWithSelectUsingQueue();
            });
        }
        public event Action<Socket> ConnectionClosedEvent;
        private Queue<Socket> clientQueue = new Queue<Socket>();

        /// <summary>
        /// 从已连接Socket队列中取出Socket并处理
        /// </summary>
        private void FetchFromQueue()
        {
            ThreadPool.QueueUserWorkItem(obj =>
            {
                while (true)
                {
                    if (clientQueue.Count == 0)
                        continue;
                    var s = clientQueue.Dequeue();
                    if (s == null)
                        continue;
                    var remoteEnd = s.RemoteEndPoint as IPEndPoint;
                    bool r = true;
                    if (ReceiveEvent != null)
                    {
                        r = ReceiveEvent(s, remoteEnd);//既然Socket.Select已经返回，故此处不会阻塞
                    }
                    if (!r)
                    {
                        allSet.Remove(s);
                        AcceptedSockets.Remove(s);
                        closedClients.Add(s);
                        if (ConnectionClosedEvent != null)
                            ConnectionClosedEvent(s);
                        Console.WriteLine(">{0}:{1}已断开连接", remoteEnd.Address.ToString(), remoteEnd.Port);
                    }
                }
            });
        }
        List<Socket> allSet = new List<Socket>();
        List<Socket> rSet = new List<Socket>();//需要监控有无可读数据的Socket集合
        List<Socket> listClients = new List<Socket>();//已连接的Socket集合
        List<Socket> closedClients = new List<Socket>();//暂存已断开连接的Socket集合
        /// <summary>
        /// 多路复用I/O
        /// </summary>
        private void WatchTcpConnectionWithSelect()
        {
            allSet.Clear();
            allSet.Add(tcpListenSock);
            listClients.Clear();
            
            while (true)
            {
                rSet.Clear();
                rSet.AddRange(allSet);//rSet在一次循环中保持不变，直到下次循环被allSet重新赋值

                closedClients.Clear();
                

                Socket.Select(rSet, null, null, -1);//调用Select之后，没有可读数据的Socket会被从allset中删除

                if (rSet.Contains(tcpListenSock))//判断有没有新连接传入
                {
                    var workerSock = tcpListenSock.Accept();//既然Socket.Select已经返回，故此处不会阻塞
                    allSet.Add(workerSock);
                    listClients.Add(workerSock);
                    AcceptedSockets.Add(workerSock);

                    var remoteEnd = workerSock.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(">Client {0}:{1} connected...", remoteEnd.Address.ToString(), remoteEnd.Port);
                    rSet.Remove(tcpListenSock);
                    if (rSet.Count == 0)
                        continue;
                }
                
                foreach (var s in listClients)//接收已连接的数据
                {
                    if (rSet.Contains(s))
                    {
                        var remoteEnd = s.RemoteEndPoint as IPEndPoint;
                        bool r = true;
                        if (ReceiveEvent != null)
                        {
                            r = ReceiveEvent(s, remoteEnd);//既然Socket.Select已经返回，故此处不会阻塞
                        }
                        if (!r)
                        {
                            allSet.Remove(s);
                            AcceptedSockets.Remove(s);
                            closedClients.Add(s);
                            if (ConnectionClosedEvent != null)
                                ConnectionClosedEvent(s);
                            Console.WriteLine(">{0}:{1}已断开连接", remoteEnd.Address.ToString(), remoteEnd.Port);
                        }
                    }
                }
                foreach (var c in closedClients)
                {
                    listClients.Remove(c);
                }
            }
        }

        private void WatchTcpConnectionWithSelectUsingQueue()
        {
            allSet.Clear();
            allSet.Add(tcpListenSock);
            listClients.Clear();

            while (true)
            {
                rSet.Clear();
                rSet.AddRange(allSet);//rSet在一次循环中保持不变，直到下次循环被allSet重新赋值

                closedClients.Clear();

                try
                {
                    Socket.Select(rSet, null, null, -1);//调用Select之后，没有可读数据的Socket会被从allset中删除
                }
                catch(Exception ee)
                {
                    Console.WriteLine(ee.Message);
                    continue;
                }

                if (rSet.Contains(tcpListenSock))//判断有没有新连接传入
                {
                    var workerSock = tcpListenSock.Accept();//既然Socket.Select已经返回，故此处不会阻塞
                    allSet.Add(workerSock);
                    listClients.Add(workerSock);
                    AcceptedSockets.Add(workerSock);

                    var remoteEnd = workerSock.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(">Client {0}:{1} connected...", remoteEnd.Address.ToString(), remoteEnd.Port);
                    rSet.Remove(tcpListenSock);
                    if (rSet.Count == 0)
                        continue;
                }

                foreach (var s in listClients)//接收已连接的数据
                {
                    if (rSet.Contains(s))
                    {
                        try
                        {
                            if (!clientQueue.Contains(s))
                                clientQueue.Enqueue(s);
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee.Message);
                        }
                    }
                }
                //foreach (var c in closedClients)
                //{
                //    listClients.Remove(c);
                //}
            }
        }

        /// <summary>
        /// 多线程同步阻塞模式
        /// </summary>
        private void WatchTcpConnectionWithMultiThread()
        {
            while (true)
            {
                var workerSock = tcpListenSock.Accept();
                AcceptedSockets.Add(workerSock);
                var remoteEnd = workerSock.RemoteEndPoint as IPEndPoint;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(">Client {0}:{1} connected...", remoteEnd.Address.ToString(), remoteEnd.Port);


                //接收
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    while (true)
                    {
                        bool r = true;
                        if (ReceiveEvent != null)
                            r = ReceiveEvent(workerSock, remoteEnd);
                        if (!r)
                            break;
                    }
                    AcceptedSockets.Remove(workerSock);
                    if (ConnectionClosedEvent != null)
                        ConnectionClosedEvent(workerSock);
                    Console.WriteLine(">{0}:{1}已断开连接", remoteEnd.Address.ToString(), remoteEnd.Port);
                });
            }
        }

        private void WatchTcpConnectionWithSingleThread()
        {
            while (true)
            {
                var workerSock = tcpListenSock.Accept();
                AcceptedSockets.Add(workerSock);
                var remoteEnd = workerSock.RemoteEndPoint as IPEndPoint;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(">Client {0}:{1} connected...", remoteEnd.Address.ToString(), remoteEnd.Port);

                //接收
                while (true)
                {
                    bool r = true;
                    if (ReceiveEvent != null)
                        r = ReceiveEvent(workerSock, remoteEnd);
                    if (!r)
                        break;
                }

                AcceptedSockets.Remove(workerSock);
                if (ConnectionClosedEvent != null)
                    ConnectionClosedEvent(workerSock);
                Console.WriteLine(">{0}:{1}已断开连接", remoteEnd.Address.ToString(), remoteEnd.Port);
            }
        }
    }
}
