using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace RealServer.ProcessReceive
{
    public interface IProcessReceive
    {
        void Process(Socket workerSock);
    }
}
