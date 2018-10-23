using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace SocketLib.Msg
{
    public class GetFriendMsg : MsgBase
    {
        public int UserNameLen { set; get; }
        public string UserName { set; get; }

        public GetFriendMsg(string uName)
        {
            List<Byte> list = new List<byte>();
            list.Add((byte)Cmd);//命令类型
            var bufUserName = Encoding.UTF8.GetBytes(uName);
            var bufUserNameLen = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(bufUserName.Length));
            list.AddRange(bufUserNameLen);//用户名长度
            list.AddRange(bufUserName);//用户名

            Bytes = list.ToArray();
        }

        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.GET_FRIENDS;
            }
        }
        public GetFriendMsg()
        {
            Bytes = new byte[] { (byte)Cmd };
        }

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
