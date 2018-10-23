using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib;
using SocketLib.Msg;
using System.Timers;

namespace RealClient.ReceiveFromTcp
{
    public class ProcessLoginResult : IProcessReceive
    {
        private Socket sock = null;
        public void Process(Socket workerSock)
        {
            sock = workerSock;
            byte[] bufLoginResult = new byte[1];
            workerSock.Receive(bufLoginResult);
            var loginResult = (LOGIN_RESULT)bufLoginResult[0];
            switch (loginResult)
            {
                case LOGIN_RESULT.OK:
                    Console.WriteLine("Login OK");
                    UserService.GetFriends(workerSock);
                    UserService.LoginOk = true;
                    break;
                case LOGIN_RESULT.PWD_ERROR:
                    Console.WriteLine("Login Failed");
                    UserService.LoginOk = false;
                    break;
                case LOGIN_RESULT.NOT_LOGIN_YET:
                    Console.WriteLine("You havn't login yet!");
                    break;
                case LOGIN_RESULT.USER_NOT_EXIST:
                    break;
            }
        }
    }
}
