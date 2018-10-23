using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketLib
{
    public enum PROTOCOL_CMD
    {
        LOGIN = 0x01,
        TXTMSG = 0x02,
        FILEMSG = 0x03,
        LOGIN_RESULT = 0x04,
        PROTOCAL_ILLEGAL = 0x05,
        GET_FRIENDS = 0x06,
        FRIENDS_LIST = 0x07,
        LOGOUT = 0x08,
        REGIST = 0x09,
        REGIST_RESULT = 0x0A,
        NEW_USER_ONLINE = 0x0B,
        HEART_BEAT = 0x0C,
        DOWNLOAD_FACES = 0x0D
    }
}
