using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SocketLib;
using RealServer.ProcessReceive;
using SocketLib.Msg;
using System.IO;
using SocketLib.Models;

namespace RealServer
{
   
    public class Server
    {
        private Socket udpListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public static TcpHelper TheTcpHelper
        {
            get
            {
                return tcp;
            }
        }
        private static TcpHelper tcp = null;

        public Server(int port = 9996)
        {
            Console.Title = "IM Server";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">You may input 【$h】 for usage!");

            tcp = new TcpHelper(port);
            tcp.ReceiveEvent += new Func<Socket, IPEndPoint, bool>(tcpHelper_ReceiveEvent);
            tcp.ConnectionClosedEvent += new Action<Socket>(Tcp_ConnectionClosedEvent);
        }

        void Tcp_ConnectionClosedEvent(Socket obj)
        {
            var u = UserService.GetUserInfoFromSocket(obj);
            if (u == null)
                return;
            var userService = new UserService(u.UserName);
            userService.OffLine();
        }

        bool tcpHelper_ReceiveEvent(Socket arg1, IPEndPoint arg2)
        {
            return InternalReceive(arg1, arg2);
        }

        public void StartServer()
        {
            
            tcp.Listen();
                       
            SendHeartBeat();

            ProcessUserInput();
        }
        
        private void SendHeartBeat()
        {
            HeartBeat heartBeat = new HeartBeat(tcp.AcceptedSockets, 60000, 60000);
            heartBeat.UserOfflineEvent += new Action<OnlineUserInfo>(heartBeat_UserOfflineEvent);
            heartBeat.Start();
        }

        void heartBeat_UserOfflineEvent(OnlineUserInfo obj)
        {
            var userService = new UserService(obj.UserName);
            userService.OffLine();
        }

        private void ProcessUserInput()
        {
            while (true)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "$q":
                        return;
                    case "$lc":
                        ListClients();
                        break;
                    case "$lu":
                        ListOnLineUsers();
                        break;
                    case "$c":
                        Console.Clear();
                        break;
                    case "$h":
                        ShowHelp();
                        break;
                    default:
                        SendToAllClients(userInput);
                        break;
                }
            }
        }
        private void ListOnLineUsers()
        {
            Console.WriteLine("================OnlineUsers================");
            foreach (var s in UserService.LoginUsers)
            {
                Console.WriteLine(s.UserName);
            }
            Console.WriteLine("===========================================");
        }
        private void ListClients()
        {
            Console.WriteLine("================Clients================");
            foreach (var s in tcp.AcceptedSockets)
            {
                Console.WriteLine(s.RemoteEndPoint.ToString());
            }
            Console.WriteLine("=======================================");
        }
        
        private bool InternalReceive(Socket workerSock, IPEndPoint remoteEnd)
        {
            byte[] bufCmd = new byte[1];
            try
            {
              
                var c = workerSock.Receive(bufCmd);
                if (c == 0)
                    return false;
            }
            catch
            {
                return false;
            }
            PROTOCOL_CMD cmd = (PROTOCOL_CMD)bufCmd[0];
            IProcessReceive processReceive = null;
            switch (cmd)
            {
                case PROTOCOL_CMD.LOGIN:
                    processReceive = new ProcessLogin();
                    break;
                case PROTOCOL_CMD.PROTOCAL_ILLEGAL:
                    processReceive = new ProcessProtocolIllegal();
                    break;
                case PROTOCOL_CMD.TXTMSG:
                    processReceive = new ProcessTxtMsg();
                    break;
                case PROTOCOL_CMD.FILEMSG:
                    processReceive = new ProcessFileMsg();
                    break;
                case PROTOCOL_CMD.GET_FRIENDS:
                    processReceive = new ProcessGetFriends();
                    break;
                case PROTOCOL_CMD.LOGOUT:
                    processReceive = new ProcessLogout();
                    break;
                case PROTOCOL_CMD.REGIST:
                    processReceive = new ProcessRegist();
                    break;
                case PROTOCOL_CMD.HEART_BEAT:
                    processReceive = new ProcessHeartBeat();
                    break;
                case PROTOCOL_CMD.DOWNLOAD_FACES:
                    processReceive = new ProcessDownloadFaces();
                    break;
                default: 
                    break;
            }
            if (processReceive != null)
                processReceive.Process(workerSock);
            return true;
        }

        private void SendToAllClients(string txt)
        {
            if (tcp.AcceptedSockets == null || tcp.AcceptedSockets.Count == 0)
            {
                Console.WriteLine(">No client available");
                return;
            }
            TxtMsg txtMsg = new TxtMsg(txt, "Server");
            foreach (var sock in tcp.AcceptedSockets)
            {
                txtMsg.Send(sock);
            }
        }
        private void ShowHelp()
        {
            Console.WriteLine(">You may use these commands:");
            Console.WriteLine("【$lc】:List clients");
            Console.WriteLine("【$lu】:List online users");
            Console.WriteLine("【$c】:Clear screen");
            Console.WriteLine("【$q】:Quit app");
        }
    }
}
