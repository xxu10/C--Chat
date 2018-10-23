using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib.Msg;
using System.Net.Sockets;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessHeartBeat : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            if (!UserService.LoginOk)
                return;

            HeartBeatMsg msg = new HeartBeatMsg();
            msg.ReceiveFrom(workerSock);
            Console.WriteLine("Heart beat from {0}", msg.UserName);

            HeartBeatMsg heart = new HeartBeatMsg(UserService.LoginUserName);
            heart.Send(workerSock);

            
        }
    }
}
