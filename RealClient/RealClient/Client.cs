using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using SocketLib;
using System.Threading;
using System.IO;
using RealClient.ReceiveFromTcp;
using SocketLib.Msg;
using SocketLib.Models;

namespace RealClient
{
    public class Client
    {
        private Socket serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint serverEndPoint = null;
        private P2P p2p = null;
        private IFace face = null;

        public Client(IPEndPoint serverEnd,IFace fc)
        {
            serverEndPoint = serverEnd;
            face = fc;
            p2p = new P2P(face, this);

            Console.Title = "IM Client";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">You may input 【$h】 for usage!");
        }

        public void Start()
        {
            if (!ConnectToServer())
            {
                Console.WriteLine("Unable to connect server.Retrying...");
                Start();
                return;
            }

            DownLoadFaces();

            Login();

            p2p.Start();

            ProcessUserInput();
        }
        private void DownLoadFaces()
        {
            DownloadFacesMsg msg = new DownloadFacesMsg("Faces");
            msg.Send(serverSock);
        }
        private bool ConnectToServer()
        {
            try
            {
                serverSock.Connect(serverEndPoint);
                Console.WriteLine(">Connected to server【{0}:{1}】 OK", serverEndPoint.Address.ToString(), serverEndPoint.Port);
                var localEnd = serverSock.LocalEndPoint as IPEndPoint;
                Console.WriteLine(">Local EndPont is {0}:{1}", localEnd.Address.ToString(), localEnd.Port);
                Console.WriteLine(Environment.NewLine);

                ThreadPool.QueueUserWorkItem(obj =>
                {
                    while (true)
                    {
                        if (!IntenalReceive())
                            break;
                    }
                    Console.WriteLine(">{0}:{1}has already disconnected ...", serverEndPoint.Address.ToString(), serverEndPoint.Port);
                });
                return true;
            }
            catch (Exception ee)
            {
                //Console.WriteLine(">" + ee.Message);
                return false;
            }
        }
        private bool IntenalReceive()
        {
            byte[] bufCmd = new byte[1];
            try
            {
                serverSock.Receive(bufCmd);
            }
            catch
            {
                return false;
            }
            PROTOCOL_CMD cmd = (PROTOCOL_CMD)bufCmd[0];
            IProcessReceive processReceive = null;
            switch (cmd)
            {
                case PROTOCOL_CMD.LOGIN_RESULT:
                    processReceive = new ProcessLoginResult();
                    break;
                case PROTOCOL_CMD.PROTOCAL_ILLEGAL:
                    processReceive = new ProcessProtocolIllegal();
                    break;
                case PROTOCOL_CMD.TXTMSG:
                    processReceive = new ProcessTxtMsg();
                    break;
                case PROTOCOL_CMD.FRIENDS_LIST:
                    processReceive = new ProcessFriendList();
                    break;
                case PROTOCOL_CMD.REGIST_RESULT:
                    processReceive = new ProcessRegistResult();
                    break;
                case PROTOCOL_CMD.NEW_USER_ONLINE:
                    processReceive = new ProcessNewUserOnline();
                    break;
                case PROTOCOL_CMD.HEART_BEAT:
                    processReceive = new ProcessHeartBeat();
                    break;
                case PROTOCOL_CMD.LOGOUT:
                    processReceive = new ProcessLogout();
                    break;
                case PROTOCOL_CMD.FILEMSG:
                    processReceive = new ProcessFileMsg(AppDomain.CurrentDomain.BaseDirectory + @"Faces");
                    break;
            }
            if (processReceive != null)
                processReceive.Process(serverSock);
            return true;
        }
        private void RegistNewUser()
        {
            Console.WriteLine(">Please input your user name:");
            var uName = Console.ReadLine();
            Console.WriteLine(">Please input your password:");
            var uPwd = Console.ReadLine();
            Console.WriteLine(">Please confirm your password:");
            var uPwd2 = Console.ReadLine();
            if (uPwd != uPwd2)
            {
                Console.WriteLine(">Confirm password not match!");
                return;
            }

            UserService.RegistUserName = uName;
            RegistMsg msg = new RegistMsg(uName, uPwd);
            msg.Send(serverSock);
        }
        private void ProcessUserInput()
        {
            while (true)
            {
                var userInput = Console.ReadLine();
                if (userInput == "$cu")
                {
                    if (UserService.LoginOk)
                    {
                        Console.WriteLine("Current login user is【{0}】", UserService.LoginUserName);
                    }
                    else
                    {
                        Console.WriteLine("You havn't login yet!");
                    }
                    continue;
                }
                if (userInput == "$r")
                {
                    RegistNewUser();
                    continue;
                }
                if (userInput == "$h")
                {
                    ShowHelp();
                    continue;
                }
                if (userInput == "$c")
                {
                    Console.Clear();
                    continue;
                }
                if (userInput == "$li")
                {
                    if (UserService.LoginOk)
                        Console.WriteLine(">You have login already!");
                    else
                        Login();
                    continue;
                }
                if (userInput == "$q")
                {
                    break;
                }
                if (!UserService.LoginOk)
                {
                    Login();
                    continue;
                }
                switch (userInput)
                {
                    case "$lo":
                        Logout();
                        break;
                    case "$sf":
                        SendFile();
                        break;
                    case "$gf":
                        UserService.GetFriends(serverSock);
                        ShowFriends();
                        break;
                    case "$ch":
                        SendTxtToFriend();
                        break;
                    case "$fc":
                        SendFaceToFriend();
                        break;
                    case "$sf2":
                        SendFile();
                        break;
                    case "$s":
                        SendMsgToServer();
                        break;
                }
            }
        }
        
        private void SendFaceToFriend()
        {
            Console.WriteLine(">Select to send a face:");
            Console.WriteLine(face.DisplayMe());
            var faceSelect = Console.ReadLine().ToLower();
            if (!face.Contains(faceSelect))
            {
                Console.WriteLine("Illegal face!");
                return;
            }

            Console.WriteLine(">Who do you want to send txt to?");
            var friendName = Console.ReadLine();
            if (p2p.CheckFriend(friendName) == null)
            {
                Console.WriteLine("Friend not exists,or not online!");
                return;
            }

            p2p.SendTxtToFriend(friendName, faceSelect);
        }
        public void SendMsgToServer()
        {
            Console.WriteLine("Input your msg to be sent to server:");
            var userInput = Console.ReadLine();
            TxtMsg txtMsg = new TxtMsg(userInput, UserService.LoginUserName);
            txtMsg.Send(serverSock);
        }
        private void SendTxtToFriend()
        {
            Console.WriteLine(">Who do you want to send txt to?");
            var friendName = Console.ReadLine();
            if (p2p.CheckFriend(friendName) == null)
            {
                Console.WriteLine("Friend not exists!");
                return;
            }
            Console.WriteLine(">Please input your message:");
            string msg = Console.ReadLine();
            p2p.SendTxtToFriend(friendName, msg);
        }

        private static void ShowFriends()
        {
            Console.WriteLine("==========================Friends==========================");
            foreach (var u in UserService.CurrentUsersFriend)
            {
                string r = null;
                if (u.IPEnd == null)
                    r = u.UserName + "【OffLine】";
                else
                    r = u.UserName + "【" + u.IPEnd.Address.ToString() + "】";
                Console.WriteLine(r);
            }
            Console.WriteLine("===========================================================");
        }
        
        private void Logout()
        {
            UserService.LoginOk = false;
            LogoutMsg msg = new LogoutMsg(UserService.LoginUserName);
            msg.Send(serverSock);
            Console.WriteLine(">Logout successfully!");
        }
        private void ShowHelp()
        {
            Console.WriteLine(">You may use these commands:");
            Console.WriteLine("【$li】:Login");
            Console.WriteLine("【$lo】:Logout");
            Console.WriteLine("【$r】:Register new user");
            Console.WriteLine("【$sf】:Send file");
            Console.WriteLine("【$c】:Clear screen");
            Console.WriteLine("【$gf】:Get friends");
            Console.WriteLine("【$ch】:Chat with friend");
            Console.WriteLine("【$fc】:Send face to friend");
            Console.WriteLine("【$ca】:Cancel chat with friend");
            Console.WriteLine("【$s】:Send msg to server");
            Console.WriteLine("【$q】:Quit app");
        }

        private void Login()
        {
            Console.WriteLine(">LOGIN:");
            Console.WriteLine(">Please input your user name:");
            var uName = Console.ReadLine();
            UserService.LoginUserName = uName;
            Console.WriteLine(">Please input your password:");
            var uPwd = Console.ReadLine();

            LoginMsg msg = new LoginMsg(uName, uPwd);
            msg.Send(serverSock);
        }
        private void SendFile()
        {
            Console.WriteLine(">Who do you want to send file to?");
            var friendName = Console.ReadLine();
            if (friendName == "$ca")
            {
                Console.WriteLine(">Send file canceled!");
                return;
            }
            if (p2p.CheckFriend(friendName) == null)
            {
                SendFile();
                return;
            }
            Console.WriteLine(">Please select your file:");
            var filePath = Console.ReadLine();
            p2p.SendFile(friendName, filePath);
        }
        private void SendFile(string filePath = @"E:\Jellyfish.jpg", int step = 8192)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine(">File {0} not exist!", filePath);
                return;
            }
            FileMsg msg = new FileMsg(filePath, UserService.LoginUserName, step);
            var r = msg.Send(serverSock);
            Console.WriteLine(r ? ">File send complete!" : ">Lost connection!");
        }
    }
}
