using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib;
using SocketLib.Models;
using System.Net;
using System.Net.Sockets;
using SocketLib.Msg;

namespace RealServer
{
    public class UserService
    {
        public static List<UserInfo> Users
        {
            get
            {
                List<UserInfo> list = new List<UserInfo>();
                list.Add(new UserInfo { Name = "Jim", Pwd = "Jim" });
                list.Add(new UserInfo { Name = "Lily", Pwd = "Lily" });
                list.Add(new UserInfo { Name = "John", Pwd = "John" });
                list.Add(new UserInfo { Name = "Tom", Pwd = "Tom" });
                list.Add(new UserInfo { Name = "Gates", Pwd = "Gates" });
                list.Add(new UserInfo { Name = "Green", Pwd = "Green" });
                return list;
            }
        }

        public static List<Friends> Friends
        {
            get
            {
                List<Friends> r = new List<Friends>();
                r.Add(new Friends { UserName = "Jim", FriendName = "Lily" });
                r.Add(new Friends { UserName = "Jim", FriendName = "John" });
                r.Add(new Friends { UserName = "John", FriendName = "Lily" });
                r.Add(new Friends { UserName = "Lily", FriendName = "Jim" });
                r.Add(new Friends { UserName = "Lily", FriendName = "John" });
                r.Add(new Friends { UserName = "Lily", FriendName = "Green" });
                r.Add(new Friends { UserName = "Gates", FriendName = "Jim" });
                r.Add(new Friends { UserName = "Gates", FriendName = "John" });
                r.Add(new Friends { UserName = "Tom", FriendName = "Green" });
                r.Add(new Friends { UserName = "Green", FriendName = "John" });
                return r;
            }
        }

        private string uName;
        private string uPwd;
        public UserService(string userName, string userPwd = null)
        {
            uName = userName;
            uPwd = userPwd;
        }
        public IPAddress GetIP()
        {
            var user = LoginUsers.Find(u => u.UserName == uName);
            return user == null ? null : user.IPEnd.Address;
        }
        public bool CheckLogin()
        {
            var r = Users.Find(u => u.Name == uName && u.Pwd == uPwd);
            return r != null;
        }
        public List<OnlineUserInfo> GetFriends()
        {
            List<OnlineUserInfo> list = new List<OnlineUserInfo>();
            var friends = Friends.FindAll(x => x.UserName == uName);
            foreach (Friends f in friends)
            {
                var u = new OnlineUserInfo();
                u.UserName = f.FriendName;
                var x = LoginUsers.Find(user => user.UserName == u.UserName);
                u.IPEnd = x == null ? null : x.IPEnd;
                list.Add(u); 
            }
            return list;
        }
        public void OffLine()
        {
            Console.WriteLine("{0} is offline!", uName);

            var uu = LoginUsers.Find(a => a.UserName == uName);
            if (uu != null)
                LoginUsers.Remove(uu);

            if (DicUserSockets.ContainsKey(uName))
                DicUserSockets.Remove(uName);

            var userService = new UserService(uName);
            var theFriends = userService.GetFriends();
            foreach (var f in theFriends)
            {
                if (DicUserSockets.ContainsKey(f.UserName))
                {
                    LogoutMsg logout = new LogoutMsg(uName);
                    var sock = DicUserSockets[f.UserName];
                    logout.Send(sock);
                }
            }
        }
        public bool CheckOnLine()
        {
            return LoginUsers.Exists(u => u.UserName == uName);
        }

        private static Dictionary<string, Socket> _DicUserSockets = new Dictionary<string, Socket>();
        public static Dictionary<string, Socket> DicUserSockets
        {
            get
            {
                return _DicUserSockets;
            }
        }

        private static List<OnlineUserInfo> _LoginUsers = new List<OnlineUserInfo>();
        public static List<OnlineUserInfo> LoginUsers
        {
            get
            {
                return _LoginUsers;
            }
        }
        private static Dictionary<string, bool> _DicUserOnLineState = new Dictionary<string, bool>();
        public static Dictionary<string, bool> DicUserOnLineState
        {
            get
            {
                return _DicUserOnLineState;
            }
        }
        public static OnlineUserInfo GetUserInfoFromSocket(Socket sock)
        {
            var remote = sock.RemoteEndPoint as IPEndPoint;
            return LoginUsers.Find(u => u.IPEnd == remote);
        }
    }
}
