using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace SocketLib.Msg
{
    public class LogoutMsg:MsgBase
    {
        public string UserName { set; get; }
        public int UserNameLen { set; get; }

        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.LOGOUT;
            }
        }
        public LogoutMsg(string uName)
        {
            UserName = uName;

            List<Byte> list = new List<byte>();
            list.Add((byte)Cmd);

            var bufUserName = Encoding.UTF8.GetBytes(UserName);
            UserNameLen = IPAddress.HostToNetworkOrder(bufUserName.Length);
            var bufUserNameLen = BitConverter.GetBytes(UserNameLen);
            list.AddRange(bufUserNameLen);
            list.AddRange(bufUserName);

            Bytes = list.ToArray();
        }
        public LogoutMsg() { }

        public override void ReceiveFromStream(Stream stream)
        {
            byte[] bufUserNameLen = new byte[4];
            stream.Read(bufUserNameLen, 0, bufUserNameLen.Length);
            UserNameLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufUserNameLen, 0));

            byte[] bufUserName = new byte[UserNameLen];
            stream.Read(bufUserName, 0, bufUserName.Length);
            UserName = Encoding.UTF8.GetString(bufUserName);
        }
    }
}
