using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace RealClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint serverEnd = new IPEndPoint(IPAddress.Parse("130.215.210.127"), 9996);
            Client client = new Client(serverEnd, new FaceInFile());
            client.Start();
        }
    }
}
