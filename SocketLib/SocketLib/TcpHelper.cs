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
                        r = ReceiveEvent(s, remoteEnd);
                    }
                    if (!r)
                    {
                        allSet.Remove(s);
                        AcceptedSockets.Remove(s);
                        closedClients.Add(s);
                        if (ConnectionClosedEvent != null)
                            ConnectionClosedEvent(s);
                        Console.WriteLine(">{0}:{1}has already disconnected ...", remoteEnd.Address.ToString(), remoteEnd.Port);
                    }
                }
            });
        }
        List<Socket> allSet = new List<Socket>();
        List<Socket> rSet = new List<Socket>();
        List<Socket> listClients = new List<Socket>();
        List<Socket> closedClients = new List<Socket>();
      
        private void WatchTcpConnectionWithSelect()
        {
            allSet.Clear();
            allSet.Add(tcpListenSock);
            listClients.Clear();
            
            while (true)
            {
                rSet.Clear();
                rSet.AddRange(allSet);

                closedClients.Clear();
                

                Socket.Select(rSet, null, null, -1);

                if (rSet.Contains(tcpListenSock))
                {
                    var workerSock = tcpListenSock.Accept();
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
                
                foreach (var s in listClients)
                {
                    if (rSet.Contains(s))
                    {
                        var remoteEnd = s.RemoteEndPoint as IPEndPoint;
                        bool r = true;
                        if (ReceiveEvent != null)
                        {
                            r = ReceiveEvent(s, remoteEnd);
                        }
                        if (!r)
                        {
                            allSet.Remove(s);
                            AcceptedSockets.Remove(s);
                            closedClients.Add(s);
                            if (ConnectionClosedEvent != null)
                                ConnectionClosedEvent(s);
                            Console.WriteLine(">{0}:{1}has already disconnected ...", remoteEnd.Address.ToString(), remoteEnd.Port);
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
                rSet.AddRange(allSet);

                closedClients.Clear();

                try
                {
                    Socket.Select(rSet, null, null, -1);
                }
                catch(Exception ee)
                {
                    Console.WriteLine(ee.Message);
                    continue;
                }

                if (rSet.Contains(tcpListenSock))
                {
                    var workerSock = tcpListenSock.Accept();
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

                foreach (var s in listClients)
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
              
            }
        }

        private void WatchTcpConnectionWithMultiThread()
        {
            while (true)
            {
                var workerSock = tcpListenSock.Accept();
                AcceptedSockets.Add(workerSock);
                var remoteEnd = workerSock.RemoteEndPoint as IPEndPoint;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(">Client {0}:{1} connected...", remoteEnd.Address.ToString(), remoteEnd.Port);

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
                    Console.WriteLine(">{0}:{1}has already disconnected ...", remoteEnd.Address.ToString(), remoteEnd.Port);
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
                Console.WriteLine(">{0}:{1}has already disconnected ...", remoteEnd.Address.ToString(), remoteEnd.Port);
            }
        }
    }
}
