using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SocketLib.Msg;
using SocketLib.Models;

namespace RealServer
{
  
    public class HeartBeat
    {
        private System.Timers.Timer timerSendHeartBeat =null;
        private System.Timers.Timer timerCheckHeartBeat = null;
        private List<Socket> clientSockets = null;
        private HeartBeatMsg heartBeat = new HeartBeatMsg("Server");
        public event Action<OnlineUserInfo> UserOfflineEvent;

        public HeartBeat(List<Socket> clients,int sendInterval,int checkInterval)
        {
            clientSockets = clients;

            timerSendHeartBeat = new System.Timers.Timer(sendInterval);
            timerCheckHeartBeat = new System.Timers.Timer(checkInterval);
        }
        public void Start()
        {
            timerSendHeartBeat.Elapsed+=new System.Timers.ElapsedEventHandler(timerSendHeartBeat_Elapsed);
            timerCheckHeartBeat.Elapsed+=new System.Timers.ElapsedEventHandler(timerCheckHeartBeat_Elapsed);
            timerSendHeartBeat.Start();
        }
        private void timerCheckHeartBeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerCheckHeartBeat.Stop();
            foreach (var u in UserService.LoginUsers)
            {
                if (!UserService.DicUserOnLineState[u.UserName])
                {
                    if (UserOfflineEvent != null)
                        UserOfflineEvent(u);
                }
            }
        }
        private void timerSendHeartBeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (clientSockets == null)
                return;
            foreach (var sock in clientSockets)
            {
                heartBeat.Send(sock);

                var u = UserService.GetUserInfoFromSocket(sock);
                if (!UserService.DicUserOnLineState.Keys.Contains(u.UserName))
                    UserService.DicUserOnLineState.Add(u.UserName, false);
                else
                    UserService.DicUserOnLineState[u.UserName] = false;
            }
            timerCheckHeartBeat.Start();
        }
    }
}
