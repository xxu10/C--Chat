using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib;
using System.Net.Sockets;
using SocketLib.Msg;
using System.Net;
using SocketLib.Models;

namespace RealServer.ProcessReceive
{
    public class ProcessLogin : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            LoginMsg login = new LoginMsg();
            login.ReceiveFrom(workerSock);
            string userName = login.UserName;
            string userPwd = login.UserPwd;

            var userService = new UserService(userName, userPwd);
            bool loginResult = userService.CheckLogin();
            if (loginResult)
            {
                OnlineUserInfo user = new OnlineUserInfo { UserName = userName, IPEnd = workerSock.RemoteEndPoint as IPEndPoint };
                foreach (var s in Server.TheTcpHelper.AcceptedSockets)
                {
                    NewUserOnlineMsg newUserOnlineMsg = new NewUserOnlineMsg(user.UserName, user.IPEnd.Address.ToString());
                    newUserOnlineMsg.Send(s);
                }

                UserService.LoginUsers.Add(user);
                if (!UserService.DicUserSockets.ContainsKey(user.UserName))
                    UserService.DicUserSockets.Add(user.UserName, workerSock);
            }
            var r = loginResult ? LOGIN_RESULT.OK : LOGIN_RESULT.PWD_ERROR;
            Console.WriteLine("{0} login {1}", userName, r);
            LoginResultMsg loginResultMsg = new LoginResultMsg(r);
            loginResultMsg.Send(workerSock);
        }
    }
}
