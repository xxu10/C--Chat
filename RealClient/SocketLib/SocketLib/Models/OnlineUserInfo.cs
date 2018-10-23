using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SocketLib.Models
{
    [Serializable]
    public class OnlineUserInfo
    {
        public string UserName { set; get; }
        public IPEndPoint IPEnd { set; get; }
    }
}
