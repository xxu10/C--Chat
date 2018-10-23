using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SocketLib.Msg
{
    public abstract class MsgBase
    {
        public abstract PROTOCOL_CMD Cmd { get; }
        public byte[] Bytes { set; get; }
        public void ReceiveFrom(Socket workerSock)
        {
            Stream stream = new NetworkStream(workerSock);
            ReceiveFromStream(stream);
        }
        public abstract void ReceiveFromStream(Stream stream);
        public virtual bool Send(Socket sock)
        {
            
            return SocketHelper.SendMsg(sock, this);
        }

        public static byte[] SerializeObj(object obj)
        {
            BinaryFormatter formater = new BinaryFormatter();
            byte[] bufFriends = null;
            using (MemoryStream ms = new MemoryStream())
            {
                formater.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                bufFriends = new byte[ms.Length];
                ms.Read(bufFriends, 0, bufFriends.Length);
            }
            return bufFriends;
        }

        public static T DeSerializeObj<T>(byte[] bufList)
        {
            T r = default(T);
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bufList, 0, bufList.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                r = (T)bf.Deserialize(ms);
            }
            return r;
        }
    }
}
