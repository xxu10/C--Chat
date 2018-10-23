using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace SocketLib.Msg
{
    public class RegistMsg:MsgBase
    {
        public int UserNameLen { set; get; }
        public string UserName { set; get; }
        public int UserPwdLen { set; get; }
        public string UserPwd { set; get; }

        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.REGIST;
            }
        }
        public RegistMsg()
        { }
        public RegistMsg(string uName, string uPwd)
        {
            UserName = uName;
            UserPwd = uPwd;

            List<byte> list = new List<byte>();
            list.Add((byte)Cmd);

            var bufUserName = Encoding.UTF8.GetBytes(UserName);
            UserNameLen = IPAddress.HostToNetworkOrder(bufUserName.Length);
            var bufUserNameLen = BitConverter.GetBytes(UserNameLen);
            list.AddRange(bufUserNameLen);
            list.AddRange(bufUserName);

            var bufUserPwd = Encoding.UTF8.GetBytes(UserPwd);
            UserPwdLen = IPAddress.HostToNetworkOrder(bufUserPwd.Length);
            var bufUserPwdLen = BitConverter.GetBytes(UserPwdLen);
            list.AddRange(bufUserPwdLen);
            list.AddRange(bufUserPwd);

            Bytes = list.ToArray();
        }

        public override void ReceiveFromStream(Stream stream)
        {
            byte[] bufUserNameLen = new byte[4];
            stream.Read(bufUserNameLen, 0, bufUserNameLen.Length);
            UserNameLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufUserNameLen, 0));

            byte[] bufUserName = new byte[UserNameLen];
            stream.Read(bufUserName, 0, bufUserName.Length);
            UserName = Encoding.UTF8.GetString(bufUserName);

            byte[] bufUserPwdLen = new byte[4];
            stream.Read(bufUserPwdLen, 0, bufUserPwdLen.Length);
            UserPwdLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufUserPwdLen, 0));

            byte[] bufUserPwd = new byte[UserPwdLen];
            stream.Read(bufUserPwd, 0, bufUserPwd.Length);
            UserPwd = Encoding.UTF8.GetString(bufUserPwd);
        }
    }
}
