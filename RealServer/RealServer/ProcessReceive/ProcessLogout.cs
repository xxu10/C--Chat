using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;

namespace RealServer.ProcessReceive
{
    public class ProcessLogout:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            LogoutMsg msg = new LogoutMsg();
            msg.ReceiveFrom(workerSock);

            //var logoutUser = UserService.LoginUsers.Find(u => u.UserName == msg.UserName);
            //UserService.LoginUsers.Remove(logoutUser);
            //if (UserService.DicUserSockets.ContainsKey(logoutUser.UserName))
            //    UserService.DicUserSockets.Remove(logoutUser.UserName);
            //Console.WriteLine("User 【{0}】 logout", msg.UserName);

            var userService = new UserService(msg.UserName);
            userService.OffLine();
        }
    }
}
