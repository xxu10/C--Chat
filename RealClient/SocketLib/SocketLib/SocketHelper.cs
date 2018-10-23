using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using SocketLib.Msg;

namespace SocketLib
{
    public class SocketHelper
    {
        private static int SendString(Socket sock, string str)
        {
            byte[] sendBuf = Encoding.UTF8.GetBytes(str);
            return SendBytes(sock, sendBuf);
        }
        internal static int SendBytes(Socket sock, byte[] sendBuf)
        {
            if (sendBuf == null || sendBuf.Length == 0)
                return 0;
            try
            {
                return sock.Send(sendBuf);
            }
            catch
            {
                var remote = sock.RemoteEndPoint as IPEndPoint;
                Console.WriteLine(">{0}:{1}已断开连接", remote.Address.ToString(), remote.Port);
                return 0;
            }
        }
        internal static bool SendMsg(Socket sock, MsgBase msg)
        {
            int r = SendBytes(sock, msg.Bytes);
            return r > 0;
        }
        public static void ReadStreamByStep(Stream stream, Action<byte[]> actOnStep, int step = 1024)
        {
            byte[] buf = new byte[step];
            do
            {
                int r = stream.Read(buf, 0, buf.Length);
                if (r <= 0)
                    break;
                if (r < buf.Length)
                    buf = buf.Take(r).ToArray();
                if (actOnStep != null)
                    actOnStep(buf);
            } while (true);
            stream.Close();
        }
    }
}
