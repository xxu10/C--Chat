using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using SocketLib;
using SocketLib.Msg;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessTxtMsg : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            TxtMsg m = new TxtMsg();
            m.ReceiveFrom(workerSock);
            
            var remote = workerSock.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("【{0}:{1}】{2} from {3}", remote.Address.ToString(), remote.Port, m.Txt, m.SenderName);
        }
    }
}
