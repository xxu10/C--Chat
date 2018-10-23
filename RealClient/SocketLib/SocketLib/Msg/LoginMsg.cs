using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace SocketLib.Msg
{
    public class LoginMsg : MsgBase
    {
        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.LOGIN;
            }
        }
        public int UserNameLen { set; get; }
        public string UserName { set; get; }
        public int UserPwdLen { set; get; }
        public string UserPwd { set; get; }

        public LoginMsg(string uName, string uPwd)
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
        
        public LoginMsg() { }

        public override void ReceiveFromStream(Stream stream)
        {
            byte[] bufUserNameLength = new byte[4];
            stream.Read(bufUserNameLength, 0, bufUserNameLength.Length);
            UserNameLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufUserNameLength, 0));

            if (!Common.CheckInt32(UserNameLen))
            {
                stream.Close();
                return;
            }

            byte[] bufUserName = new byte[UserNameLen];
            stream.Read(bufUserName, 0, bufUserName.Length);
            UserName = Encoding.UTF8.GetString(bufUserName);
            byte[] bufPwdLen = new byte[4];
            stream.Read(bufPwdLen, 0, bufPwdLen.Length);
            UserPwdLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufPwdLen, 0));
            
            if (!Common.CheckInt32(UserPwdLen))
            {
                stream.Close();
                return;
            }

            byte[] bufPwd = new byte[UserPwdLen];
            stream.Read(bufPwd, 0, bufPwd.Length);
            UserPwd = Encoding.UTF8.GetString(bufPwd);
        }
    }
}
