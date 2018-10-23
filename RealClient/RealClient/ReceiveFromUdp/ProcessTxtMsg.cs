using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLib.Msg;
using System.Net.Sockets;
using System.IO;

namespace RealClient.ReceiveFromUdp
{
    public class ProcessTxtMsg : IProcessReceive
    {
        private IFace face = null;

        public ProcessTxtMsg(IFace fc)
        {
            face = fc;
        }
        public void Process(Stream stream)
        {
            TxtMsg msg = new TxtMsg();
            msg.ReceiveFromStream(stream);
            
            var content = face.FindFaceAndReplace(msg.Txt);
            Console.WriteLine(">【{0}】：{1}", msg.SenderName, content);
        }
    }
}
