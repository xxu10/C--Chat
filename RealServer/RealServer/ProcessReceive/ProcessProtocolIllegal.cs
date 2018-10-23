using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib;
using System.Net.Sockets;

namespace RealServer.ProcessReceive
{
    public class ProcessProtocolIllegal : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            Console.WriteLine("Client protocol illegal!");

            byte[] bufProtocolIllegal = new byte[1];
            bufProtocolIllegal[0] = (byte)PROTOCOL_CMD.PROTOCAL_ILLEGAL;
            workerSock.Send(bufProtocolIllegal);
        }
    }
}
