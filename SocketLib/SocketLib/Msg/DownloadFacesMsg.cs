using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;

namespace SocketLib.Msg
{
    public class DownloadFacesMsg : MsgBase
    {
        public int FaceNameListLength { set; get; }
        public string[] FaceNameList { set; get; }

        public override PROTOCOL_CMD Cmd
        {
            get { return PROTOCOL_CMD.DOWNLOAD_FACES; }
        }
        public DownloadFacesMsg(string facePath)
        {
            List<byte> list = new List<byte>();
            list.Add((byte)Cmd);

            if (Directory.Exists(facePath))
            {
                var files = Directory.GetFiles(facePath, "*.face");
                List<string> names = new List<string>();
                foreach (var n in files)
                {
                    var name = Path.GetFileNameWithoutExtension(n);
                    names.Add(name);
                }
                var faceNameListBuf = MsgBase.SerializeObj(names.ToArray());
                FaceNameListLength = faceNameListBuf.Length;
                int networkOrder = IPAddress.HostToNetworkOrder(FaceNameListLength);
                var FaceNameListLengthBuf = BitConverter.GetBytes(networkOrder);
                list.AddRange(FaceNameListLengthBuf);
                list.AddRange(faceNameListBuf);
            }
            else
            {
                var FaceNameListLengthBuf = BitConverter.GetBytes(0);
                list.AddRange(FaceNameListLengthBuf);
            }
            Bytes = list.ToArray();
        }
        public DownloadFacesMsg()
        { }
        public override void ReceiveFromStream(Stream stream)
        {
            byte[] faceNameListLengthBuf = new byte[4];
            stream.Read(faceNameListLengthBuf, 0, faceNameListLengthBuf.Length);
            int networkOrder = BitConverter.ToInt32(faceNameListLengthBuf, 0);
            FaceNameListLength = IPAddress.NetworkToHostOrder(networkOrder);
            if (FaceNameListLength > 0)
            {
                byte[] faceNameListBuf = new byte[FaceNameListLength];
                stream.Read(faceNameListBuf, 0, faceNameListBuf.Length);
                FaceNameList = DeSerializeObj<string[]>(faceNameListBuf);
            }
        }
    }
}
