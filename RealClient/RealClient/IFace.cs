using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealClient
{
    public interface IFace
    {
        bool Contains(string symbol);

        string FindFaceAndReplace(string msg);

        string DisplayMe();
    }
}
