using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using SocketLib;
using SocketLib.Msg;
using SocketLib.Models;
using RealClient.ReceiveFromTcp;

namespace RealClient
{
    public class P2P
    {
        private const int UDP_PORT = 6676;
        private const int FILE_TCP_PORT = 16676;
        private Socket udpListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IFace face = null;
        private Client client = null;

        public P2P(IFace fc,Client cli)
        {
            face = fc;
            client = cli;
        }

       
        private void ListenForFile()
        {
            TcpHelper tcp = new TcpHelper(FILE_TCP_PORT);
            tcp.ReceiveEvent += new Func<Socket, IPEndPoint, bool>(tcp_ReceiveEvent);
            tcp.Listen();
        }

        bool tcp_ReceiveEvent(Socket arg1, IPEndPoint arg2)
        {
            return InternalReceive(arg1, arg2); 
        }
        private bool InternalReceive(Socket workerSock, IPEndPoint remoteEnd)
        {
            byte[] bufCmd = new byte[1];
            try
            {
                workerSock.Receive(bufCmd);
            }
            catch
            {
                return false;
            }
            PROTOCOL_CMD cmd = (PROTOCOL_CMD)bufCmd[0];
            IProcessReceive processReceive = null;
            switch (cmd)
            {
                case PROTOCOL_CMD.FILEMSG:
                    processReceive = new ProcessFileMsg();
                    break;
            }
            if (processReceive != null)
                processReceive.Process(workerSock);
            return true;
        }
        public void Start()
        {
            ListenForFile();
            InitUdp();
        }
        public OnlineUserInfo CheckFriend(string friend)
        {
            var f = UserService.CurrentUsersFriend.Find(u => u.UserName == friend);
            if (f == null)
            {
                Console.WriteLine("You don't have this friend!");
                return null;
            }
            if (f.IPEnd == null)
            {
                Console.WriteLine(friend + " is offline");
                return null;
            }
            return f;
        }
        public void SendTxtToFriend(string friend,string msg)
        {
            var f = CheckFriend(friend);
            if (f == null)
                return;
            var ip = f.IPEnd.Address;

            //client.SendMsgToServer();

            Socket udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var remotenEnd = new IPEndPoint(ip, UDP_PORT);
            udpSock.Connect(remotenEnd);
            TxtMsg txt = new TxtMsg(msg, UserService.LoginUserName);
            txt.Send(udpSock);
        }
        public void SendFile(string friend, string filePath, int step = 8192)
        {
            var f = UserService.CurrentUsersFriend.Find(u => u.UserName == friend);
            if (f == null)
            {
                Console.WriteLine("You don't have this friend!");
                return;
            }
            if (!File.Exists(filePath))
            {
                Console.WriteLine(">File {0} not exist!", filePath);
                return;
            }
            Socket serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var end = f.IPEnd;
            end.Port = FILE_TCP_PORT;
            serverSock.Connect(end);
            FileMsg msg = new FileMsg(filePath, UserService.LoginUserName, step);
            var r = msg.Send(serverSock);
            Console.WriteLine(r ? ">File send complete!" : ">Lost connection!");
        }
        private void InitUdp()
        {
            udpListenSock.Bind(new IPEndPoint(IPAddress.Any, UDP_PORT));
            
            ThreadPool.QueueUserWorkItem(obj =>
            {
                while (true)
                {
                    byte[] buf = new byte[10000];
                    udpListenSock.Receive(buf);
                    Stream stream = new MemoryStream();
                    stream.Write(buf, 0, buf.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    byte[] bb = new byte[1];
                    stream.Read(bb, 0, bb.Length);

                    var cmdUdp = (PROTOCOL_CMD)bb[0];
                    ReceiveFromUdp.IProcessReceive processReceive = null;
                    switch (cmdUdp)
                    {
                        case PROTOCOL_CMD.TXTMSG:
                            processReceive = new ReceiveFromUdp.ProcessTxtMsg(face);
                            break;
                    }
                    if (processReceive != null)
                        processReceive.Process(stream);
                }
            });
        }
    }
}
