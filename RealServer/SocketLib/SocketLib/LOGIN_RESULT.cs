using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace SocketLib
{
    public enum LOGIN_RESULT
    {
        OK = 0x01,
        PWD_ERROR = 0x02,
        USER_NOT_EXIST = 0x03,
        NOT_LOGIN_YET = 0x04
    }
}
