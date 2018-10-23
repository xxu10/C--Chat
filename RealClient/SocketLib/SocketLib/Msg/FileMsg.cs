using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net;

namespace SocketLib.Msg
{
    public class FileMsg : MsgBase
    {
        public int FileNameLen { set; get; }
        public string FileName { set; get; }
        public long FileLen { set; get; }
        public string FilePath { set; get; }
        public string Md5 { set; get; }
        public int SenderNameLen { set; get; }
        public string SenderName { set; get; }

        private string savePath;
        private int stepX;

        public FileMsg(string saveFile)
        {
            savePath = saveFile;
        }
        public override bool Send(Socket sock)
        {
            bool result = true;
            var fileName = Path.GetFileName(FilePath);
            using (var fs = File.OpenRead(FilePath))
            {
                #region 发送文件报文头
                List<byte> list = new List<byte>();

                list.Add((byte)Cmd);//命令类型

                var bufFileName = Encoding.UTF8.GetBytes(fileName);
                int fileNameLen = IPAddress.HostToNetworkOrder(bufFileName.Length);
                var bufFileNameLen = BitConverter.GetBytes(fileNameLen);
                list.AddRange(bufFileNameLen);//文件名长度
                list.AddRange(bufFileName);//文件名

                try
                {
                    Console.WriteLine(">Computing HASH...");
                    MD5 md5Hasher = MD5.Create();
                    var md5Code = md5Hasher.ComputeHash(fs);
                    Console.WriteLine(">Compute HASH OK");
                    list.AddRange(md5Code);//MD5校验值
                    fs.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ee)
                {
                    Console.WriteLine(">" + ee.Message);
                }

                long fileLen = IPAddress.HostToNetworkOrder(fs.Length);
                var bufFileLen = BitConverter.GetBytes(fileLen);
                list.AddRange(bufFileLen);//文件长度

                var headerArray = list.ToArray();
                SocketHelper.SendBytes(sock, headerArray); 
                #endregion

                //循环发送文件报文数据
                bool quitLoop = false;
                while (!quitLoop)
                {
                    list.Clear();

                    byte[] buf = new byte[stepX];
                    int len = fs.Read(buf, 0, buf.Length);
                    if (len < buf.Length)
                    {
                        buf = buf.Take(len).ToArray();
                        quitLoop = true;
                    }
                    list.AddRange(buf);
                    var dataArray = list.ToArray();

                    int r = SocketHelper.SendBytes(sock, dataArray);
                    if (r == 0)
                    {
                        quitLoop = true;
                        result = false;
                    }
                }

                //发送者信息
                var bufSenderName = Encoding.UTF8.GetBytes(SenderName);
                int senderNameLen = IPAddress.HostToNetworkOrder(bufSenderName.Length);
                var bufSenderNameLen = BitConverter.GetBytes(senderNameLen);
                list.Clear();
                list.AddRange(bufSenderNameLen);
                list.AddRange(bufSenderName);
                var senderArray = list.ToArray();
                SocketHelper.SendBytes(sock, senderArray);
            }
            return result;
        }
        public FileMsg(string filePath,string sender, int step = 8096)
        {
            FilePath = filePath;
            SenderName = sender;
            stepX = step;
        }

        public override PROTOCOL_CMD Cmd
        {
            get
            {
                return PROTOCOL_CMD.FILEMSG;
            }
        }

        public override void ReceiveFromStream(Stream stream)
        {
            long readStepOfFile = 8096;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            #region 接收文件报文头
            //文件名长度
            byte[] bufFileNameLen = new byte[4];
            stream.Read(bufFileNameLen, 0, bufFileNameLen.Length);
            FileNameLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufFileNameLen, 0));
            if (!Common.CheckInt32(FileNameLen))
            {
                stream.Close();
                return;
            }

            //文件名
            byte[] bufFileName = new byte[FileNameLen];
            stream.Read(bufFileName, 0, bufFileName.Length);
            FileName = Encoding.UTF8.GetString(bufFileName);

            //MD5校验值
            byte[] bufMd5 = new byte[16];
            stream.Read(bufMd5, 0, bufMd5.Length);
            Md5 = BitConverter.ToString(bufMd5);

            //文件长度
            byte[] bufFileLen = new byte[8];
            stream.Read(bufFileLen, 0, bufFileLen.Length);
            FileLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bufFileLen, 0));
            if (!Common.CheckInt64(FileLen))
            {
                stream.Close();
                return;
            }
            //if (readStepOfFile > FileLen)
            //    readStepOfFile = FileLen;
            #endregion

            //循环接收文件报文数据
            int bytesReceivedTotally = 0;
            string computedHash = null;
            string saveTo = string.Format(@"{0}\{1}", savePath, FileName);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (FileStream fs = new FileStream(saveTo, FileMode.Create))
            {
                while (true)
                {
                    if (bytesReceivedTotally >= FileLen)
                        break;
                    if (FileLen - bytesReceivedTotally < readStepOfFile)
                        readStepOfFile = FileLen - bytesReceivedTotally;
                    byte[] bufStep = new byte[readStepOfFile];
                    int bytesRead = stream.Read(bufStep, 0, bufStep.Length);
                    bytesReceivedTotally += bytesRead;
                    if (bytesRead < readStepOfFile)//即使接收缓冲区空了，过一会儿也有可能有数据过来
                    {
                        bufStep = bufStep.Take(bytesRead).ToArray();
                    }
                    fs.Write(bufStep, 0, bufStep.Length);
                    Console.WriteLine(">{0} bytes received this time;{1} bytes received totally!", bufStep.Length, bytesReceivedTotally);
                }
                stopWatch.Stop();
                Console.WriteLine(">{0} received OK,{1} bytes in {2} seconds.", FileName, FileLen, stopWatch.Elapsed.TotalSeconds);

                fs.Seek(0, SeekOrigin.Begin);
                var m = MD5.Create();
                var hash = m.ComputeHash(fs);
                computedHash = BitConverter.ToString(hash);
            }

            //发送者信息
            byte[] bufSenderNameLen = new byte[4];
            stream.Read(bufSenderNameLen, 0, bufSenderNameLen.Length);
            SenderNameLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufSenderNameLen, 0));
            byte[] bufSenderName = new byte[SenderNameLen];
            stream.Read(bufSenderName, 0, bufSenderName.Length);
            SenderName = Encoding.UTF8.GetString(bufSenderName);

            if (computedHash != Md5)
            {
                Console.WriteLine(">" + Md5 + "---------" + computedHash);
            }
        }
    }
}
