using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;
using System.Net;
using System.IO;

namespace RealServer.ProcessReceive
{
    public class ProcessDownloadFaces : IProcessReceive
    {
        public void Process(Socket workerSock)
        {
            var remote = workerSock.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("{0} want to download faces!", remote.Address.ToString());

            DownloadFacesMsg m = new DownloadFacesMsg();
            m.ReceiveFrom(workerSock);

            var files = Directory.GetFiles("Faces", "*.face");
            foreach (var file in files)
            {
                if (m.FaceNameList != null)
                {
                    var n = Path.GetFileNameWithoutExtension(file);
                    if (m.FaceNameList.Contains(n))
                        continue;
                }
                FileMsg msg = new FileMsg(file, "server");
                msg.Send(workerSock);
            }
            Console.WriteLine("{0} download faces OK!", remote.Address.ToString());
        }
    }
}
