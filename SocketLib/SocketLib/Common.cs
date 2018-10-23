using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketLib
{
    public class Common
    {
        public static bool CheckInt32(int r)
        {
            return r >= Int32.MinValue && r <= Int32.MaxValue;
        }
        public static bool CheckInt64(long r)
        {
            return r >= long.MinValue && r <= long.MaxValue;
        }
    }
}
