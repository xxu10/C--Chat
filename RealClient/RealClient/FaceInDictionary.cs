using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealClient
{
    public class FaceInDictionary : IFace
    {
        private Dictionary<string, string> dic = new Dictionary<string, string>();

        public FaceInDictionary()
        {
            InitFaces();
        }

        private void InitFaces()
        {
            dic.Add("[smile]", "[SMILE]");
            dic.Add("[cry]", "[CRY]");
            dic.Add("[angry]", "[ANGRY]");
        }
        public bool Contains(string symbol)
        {
            return dic.ContainsKey(symbol);
        }
        public string FindFaceAndReplace(string msg)
        {
            string result = msg;
            foreach (var symbol in dic.Keys)
            {
                if (result.Contains(symbol))
                {
                    result = result.Replace(symbol, dic[symbol]);
                }
            }
            return result;
        }

        public string DisplayMe()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in dic)
            {
                sb.AppendLine(string.Format("{0}==>{1}", item.Key, item.Value));
            }
            return sb.ToString();
        }
    }
}
