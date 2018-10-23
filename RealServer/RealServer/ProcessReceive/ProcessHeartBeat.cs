using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;

namespace RealServer.ProcessReceive
{
    public class ProcessHeartBeat:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            HeartBeatMsg msg = new HeartBeatMsg();
            msg.ReceiveFrom(workerSock);
            Console.WriteLine("{0} alive!", msg.UserName);
            UserService.DicUserOnLineState[msg.UserName] = true;
        }
    }
}
