using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib;
using System.Net;
using System.Net.Sockets;
using SocketLib.Msg;

namespace RealServer.ProcessReceive
{
    public class ProcessTxtMsg : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            TxtMsg txtMsg = new TxtMsg();
            txtMsg.ReceiveFrom(workerSock);
            
            var userService = new UserService(txtMsg.SenderName);
            if (!userService.CheckOnLine())
            {
                Console.WriteLine("{0} not login", txtMsg.SenderName);

                LoginResultMsg loginResultMsg = new LoginResultMsg(LOGIN_RESULT.NOT_LOGIN_YET);
                loginResultMsg.Send(workerSock);
                return;
            }
            var remote = workerSock.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("【{0}({1}:{2})】{3}", txtMsg.SenderName, remote.Address.ToString(), remote.Port, txtMsg.Txt);
        }
    }
}
