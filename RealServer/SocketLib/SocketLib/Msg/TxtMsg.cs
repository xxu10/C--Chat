using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace SocketLib.Msg
{
    public class TxtMsg : MsgBase
    {
        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.TXTMSG;
            }
        }

        public int TxtLen { set; get; }
        public string Txt { set; get; }
        public int SenderNameLen { set; get; }
        public string SenderName { set; get; }


        public TxtMsg() { }
        public TxtMsg(string txt,string senderName)
        {
            Txt = txt;
            SenderName = senderName;

            List<byte> bufMsg = new List<byte>();

            var bufTxt = Encoding.UTF8.GetBytes(Txt);
            TxtLen = IPAddress.HostToNetworkOrder(bufTxt.Length);
            
            bufMsg.Add((byte)Cmd);
            var txtLenBytes = BitConverter.GetBytes(TxtLen);
            bufMsg.AddRange(txtLenBytes);//文本消息长度
            bufMsg.AddRange(bufTxt);//文本消息

            var bufSenderName = Encoding.UTF8.GetBytes(SenderName);
            SenderNameLen = IPAddress.HostToNetworkOrder(bufSenderName.Length);
            var senderNameLenBytes = BitConverter.GetBytes(SenderNameLen);
            bufMsg.AddRange(senderNameLenBytes);//发送者用户名长度
            bufMsg.AddRange(bufSenderName);//发送者用户名

            Bytes = bufMsg.ToArray();
        }

        public override void ReceiveFromStream(Stream stream)
        {
            //协议头没考虑
            byte[] bufTxtLen = new byte[4];
            stream.Read(bufTxtLen, 0, bufTxtLen.Length);
            TxtLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufTxtLen, 0));

            if (!Common.CheckInt32(TxtLen))
            {
                stream.Close();
                return;
            }
            
            byte[] bufTxt = new byte[TxtLen];
            stream.Read(bufTxt, 0, bufTxt.Length);
            Txt = Encoding.UTF8.GetString(bufTxt);

            byte[] bufSenderNameLen = new byte[4];
            stream.Read(bufSenderNameLen, 0, bufSenderNameLen.Length);
            SenderNameLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufSenderNameLen, 0));

            if (!Common.CheckInt32(SenderNameLen))
            {
                stream.Close();
                return;
            }

            byte[] bufSenderName = new byte[SenderNameLen];
            stream.Read(bufSenderName, 0, bufSenderName.Length);
            SenderName = Encoding.UTF8.GetString(bufSenderName);
        }
    }
}
