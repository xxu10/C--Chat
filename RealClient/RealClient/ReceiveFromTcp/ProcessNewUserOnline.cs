using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;
using System.Net;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessNewUserOnline:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            NewUserOnlineMsg msg = new NewUserOnlineMsg();
            msg.ReceiveFrom(workerSock);
            if (!UserService.LoginOk)
                return;
            if (msg.UserName == UserService.LoginUserName)
                return;
            if (UserService.CurrentUsersFriend == null)
                return;
            var x = UserService.CurrentUsersFriend.Find(u => u.UserName == msg.UserName);
            if (x != null)
            {
                x.IPEnd = new IPEndPoint(IPAddress.Parse(msg.Ip), 0);
                Console.WriteLine(">New friend {0}【{1}】 online", msg.UserName, msg.Ip);
            }
        }
    }
}
