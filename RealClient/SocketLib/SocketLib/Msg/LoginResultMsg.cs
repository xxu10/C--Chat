using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace SocketLib.Msg
{
    public class LoginResultMsg : MsgBase
    {
        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.LOGIN_RESULT;
            }
        }
        public LOGIN_RESULT LoginResult { set; get; }

        public LoginResultMsg(LOGIN_RESULT r)
        {
            LoginResult = r;
            List<byte> list = new List<byte>();
            list.Add((byte)Cmd);
            list.Add((byte)LoginResult);
            Bytes = list.ToArray();
        }

        public override void ReceiveFromStream(Stream stream)
        {
            byte[] bufLoginResult = new byte[1];
            stream.Read(bufLoginResult, 0, bufLoginResult.Length);
            LoginResult = (LOGIN_RESULT)bufLoginResult[0];
        }
    }
}
