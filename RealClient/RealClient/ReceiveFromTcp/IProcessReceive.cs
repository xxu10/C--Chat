using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib;
using System.Net;

namespace RealClient.ReceiveFromTcp
{
    public interface IProcessReceive
    {
        void Process(Socket workerSock);
    }
}
