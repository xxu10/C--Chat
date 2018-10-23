using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SocketLib.Models;
using System.Net;

namespace SocketLib.Msg
{
    public class FriendsListMsg:MsgBase
    {
        public int ListLen { set; get; }
        public List<OnlineUserInfo> Friends { set; get; }

        public FriendsListMsg()
        { }
        public FriendsListMsg(List<OnlineUserInfo> friends)
        {
            List<byte> list = new List<byte>();
            list.Add((byte)Cmd);//命令类型

            var bufFriends = SerializeObj(friends);
            var bufListLen = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(bufFriends.Length));
            list.AddRange(bufListLen);//好友列表长度

            list.AddRange(bufFriends);//好友列表数据

            Bytes = list.ToArray();
        }
        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.FRIENDS_LIST;
            }
        }

        public override void ReceiveFromStream(Stream stream)
        {
            byte[] bufListLen = new byte[4];
            stream.Read(bufListLen, 0, bufListLen.Length);
            ListLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufListLen, 0));

            if (!Common.CheckInt32(ListLen))
            {
                stream.Close();
                return;
            }

            byte[] bufList = new byte[ListLen];
            stream.Read(bufList, 0, bufList.Length);
            Friends = DeSerializeObj<List<OnlineUserInfo>>(bufList);
        }
    }
}
