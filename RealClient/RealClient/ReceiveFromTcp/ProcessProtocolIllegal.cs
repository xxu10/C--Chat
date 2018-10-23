using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessProtocolIllegal : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            Console.WriteLine("Protocol illegal");
        }
    }
}
