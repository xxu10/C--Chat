using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RealClient
{
    public class FaceInFile:IFace
    {
        private const string SPLIT_LINE = "===SPLIT===";
        private string baseFacePath = @"Faces";
        private Dictionary<string, string> dic = new Dictionary<string, string>();

        public FaceInFile()
        {
            if (!Directory.Exists(baseFacePath))
                return;
            var files = Directory.GetFiles(baseFacePath, "*.face");
            foreach (var file in files)
            {
                var all = File.ReadAllText(file,Encoding.UTF8);
                var faceItems = all.Split(new string[] { SPLIT_LINE }, StringSplitOptions.None);
                foreach (var item in faceItems)
                {
                    var i = item.Trim();
                    var firstReturn = i.IndexOf(Environment.NewLine);
                    var k = i.Substring(0, firstReturn);
                    var v = i.Substring(firstReturn + Environment.NewLine.Length, i.Length - firstReturn - Environment.NewLine.Length).Trim();
                    dic.Add(k, v);
                }
            }
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
