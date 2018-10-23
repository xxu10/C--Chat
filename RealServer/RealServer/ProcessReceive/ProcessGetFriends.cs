using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib;
using System.Net;
using SocketLib.Msg;

namespace RealServer.ProcessReceive
{
    public class ProcessGetFriends:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            GetFriendMsg getFriendMsg = new GetFriendMsg();
            getFriendMsg.ReceiveFrom(workerSock);

            var remote = workerSock.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("【{0}:{1}】{2} want to get friends", remote.Address.ToString(), remote.Port, getFriendMsg.UserName);

            var userService = new UserService(getFriendMsg.UserName);
            FriendsListMsg friendsListMsg = new FriendsListMsg(userService.GetFriends());
            friendsListMsg.Send(workerSock);
        }
    }
}