using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib.Msg;
using System.Net.Sockets;
using SocketLib.Models;

namespace RealClient
{
    public class UserService
    {
        public static bool LoginOk { set; get; }
        public static string LoginUserName { set; get; }
        public static string RegistUserName { set; get; }

        public static void GetFriends(Socket serverSock)
        {
            GetFriendMsg msg = new GetFriendMsg(LoginUserName);
            msg.Send(serverSock);
        }

        public static List<OnlineUserInfo> CurrentUsersFriend { set; get; }
    }
}
