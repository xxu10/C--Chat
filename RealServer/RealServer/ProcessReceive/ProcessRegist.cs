using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;
using SocketLib;
using SocketLib.Models;

namespace RealServer.ProcessReceive
{
    public class ProcessRegist:IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            RegistMsg msg = new RegistMsg();
            msg.ReceiveFrom(workerSock);

            Console.WriteLine("New user 【{0}】 registered", msg.UserName);

            RegistResultMsg r = new RegistResultMsg(REGIST_RESULT.OK);
            r.Send(workerSock);
        }
    }
}
