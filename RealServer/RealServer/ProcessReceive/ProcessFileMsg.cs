using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib;
using SocketLib.Msg;

namespace RealServer.ProcessReceive
{
    public class ProcessFileMsg : IProcessReceive
    {
        private string saveTo;
        public ProcessFileMsg(string savePath = null)
        {
            if (savePath == null)
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                saveTo = basePath.Substring(0, basePath.Length - 1);
            }
            else
                saveTo = savePath;
        }
        public void Process(Socket workerSock)
        {
            FileMsg msg = new FileMsg(saveTo);
            msg.ReceiveFrom(workerSock);
            Console.WriteLine("File from {0}", msg.SenderName);
        }
    }
}
