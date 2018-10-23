using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessRegistResult:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            RegistResultMsg msg = new RegistResultMsg();
            msg.ReceiveFrom(workerSock);
            Console.WriteLine(">{0} registered {1}", UserService.RegistUserName, msg.RegistResult);
        }
    }
}
