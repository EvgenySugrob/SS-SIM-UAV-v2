using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Buffers.Text;
using System.Text;

namespace Vmaya.RW
{
    public class JsonUtils : MonoBehaviour
    {
        public static T FromJson<T>(string json)
        {

            return JsonUtility.FromJson<T>(json);
        }

        public static string ToJson(Object value)
        {
            return PackJson(JsonUtility.ToJson(value));
        }

        public static string PackJson(string jsonText)
        {
            int count = jsonText.Length;
            bool isCounter = false;
            int counter = 0;
            string result = "";
            for (int i = 0; i < count; i++)
            {
                if (jsonText[i] == '\\')
                {
                    if (!isCounter)
                    {
                        counter = 0;
                        isCounter = true;
                    }
                    counter++;
                }
                else
                {
                    if (isCounter)
                    {
                        isCounter = false;
                        result += '\\' + (counter > 1 ? counter.ToString() : "");
                    }
                    result += jsonText[i];
                }
            }
            return result;
        }

        public static string Unpack(string jsonText)
        {
            Regex rg = new Regex(@"(\\\d+)\D");

            MatchCollection matches = rg.Matches(jsonText);
            List<string> pass = new List<string>();
            foreach (Match m in matches)
            {
                string g = m.Groups[1].Value;
                if (!pass.Contains(g))
                {
                    string v = m.Groups[0].Value;
                    int count = int.Parse(g.Substring(1));
                    jsonText = jsonText.Replace(v, (new string('\\', count)) + v[v.Length - 1]);
                    pass.Add(m.Value);
                }
            }

            return jsonText;
        }

        internal static string Encryption(string jsonData, bool useCode = true)
        {
            if (useCode)
            {
                byte[] encodeBytes = Encoding.UTF8.GetBytes(jsonData);
                return System.Convert.ToBase64String(encodeBytes);
            }
            else return jsonData;
        }

        internal static string Unencryption(string jsonData, bool useCode = true)
        {
            if (useCode)
            {
                byte[] decodedBytes = System.Convert.FromBase64String(jsonData);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            else return jsonData;
        }
    }
}
