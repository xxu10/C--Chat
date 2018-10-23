using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RealClient.ReceiveFromUdp
{
    public interface IProcessReceive
    {
        void Process(Stream stream);
    }
}
