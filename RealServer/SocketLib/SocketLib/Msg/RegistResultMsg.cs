using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace SocketLib.Msg
{
    public class RegistResultMsg:MsgBase
    {
        public REGIST_RESULT RegistResult { set; get; }

        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.REGIST_RESULT;
            }
        }
        public RegistResultMsg() { }
        public RegistResultMsg(REGIST_RESULT arg)
        {
            RegistResult = arg;
            List<byte> list = new List<byte>();
            list.Add((byte)Cmd);
            list.Add((byte)RegistResult);
            Bytes = list.ToArray();
        }

        public override void ReceiveFromStream(Stream stream)
        {
            byte[] buf = new byte[1];
            stream.Read(buf, 0, buf.Length);
            RegistResult = (REGIST_RESULT)buf[0];
        }
    }
}
