using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib;
using SocketLib.Msg;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessFriendList : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            FriendsListMsg msg = new FriendsListMsg();
            msg.ReceiveFrom(workerSock);
            UserService.CurrentUsersFriend = msg.Friends;
        }
    }
}
