using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessLogout:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            LogoutMsg msg = new LogoutMsg();
            msg.ReceiveFrom(workerSock);

            Console.WriteLine("Friend 【{0}】 offline", msg.UserName);

            if (UserService.CurrentUsersFriend == null)
                return;
            var uu = UserService.CurrentUsersFriend.Find(u => u.UserName == msg.UserName);
            if (uu != null)
                uu.IPEnd = null;
        }
    }
}
