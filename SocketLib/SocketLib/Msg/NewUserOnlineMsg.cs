using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace SocketLib.Msg
{
    public class NewUserOnlineMsg:MsgBase
    {
        public int UserNameLen { set; get; }
        public string UserName { set; get; }
        public int IpLen { set; get; }
        public string Ip { set; get; }

        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.NEW_USER_ONLINE;
            }
        }
        public NewUserOnlineMsg() { }
        public NewUserOnlineMsg(string uName,string ip)
        {
            UserName = uName;
            Ip = ip;

            List<Byte> list = new List<byte>();
            list.Add((byte)Cmd);

            var bufUserName = Encoding.UTF8.GetBytes(UserName);
            UserNameLen = IPAddress.HostToNetworkOrder(bufUserName.Length);
            var bufUserNameLen = BitConverter.GetBytes(UserNameLen);
            list.AddRange(bufUserNameLen);
            list.AddRange(bufUserName);

            var bufIp = Encoding.UTF8.GetBytes(Ip);
            IpLen = IPAddress.HostToNetworkOrder(bufIp.Length);
            var bufIpLen = BitConverter.GetBytes(IpLen);
            list.AddRange(bufIpLen);
            list.AddRange(bufIp);

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

            byte[] bufIpLen = new byte[4];
            stream.Read(bufIpLen, 0, bufIpLen.Length);
            IpLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufIpLen, 0));

            byte[] bufIp = new byte[IpLen];
            stream.Read(bufIp, 0, bufIp.Length);
            Ip = Encoding.UTF8.GetString(bufIp);
        }
    }
}
