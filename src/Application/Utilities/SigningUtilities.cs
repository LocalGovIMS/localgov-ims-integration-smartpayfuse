using System.Collections.Generic;
using System.Linq;

namespace Application
{
    public class SigningUtilities
    {
        public static string BuildSigningString(IDictionary<string, string> dict)
        {
            Dictionary<string, string> signDict = dict.OrderBy(d => d.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

            string keystring = string.Join(":", signDict.Keys);
            string valuestring = string.Join(":", signDict.Values.Select(EscapeVal));

            return string.Format("{0}:{1}", keystring, valuestring);
        }

        private static string EscapeVal(string val)
        {
            if (val == null)
            {
                return string.Empty;
            }

            val = val.Replace(@"\", @"\\");
            val = val.Replace(":", @"\:");

            return val;
        }
    }
}
