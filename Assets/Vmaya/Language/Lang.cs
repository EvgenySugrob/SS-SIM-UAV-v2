using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Vmaya.Language
{
    public class Lang: MonoBehaviour
    {
        public string this[string index] => get(index);
        private static Lang _instance;
        private Dictionary<string, string> _items;
        public string FilePath = "language.data";

        public static Lang instance
        {
            get
            {
                if (_instance == null) 
                    _instance = (_instance = FindObjectOfType<Lang>()) ? _instance : createDefault();
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this;
            readFile();
        }

        private static Lang createDefault()
        {
            GameObject ga = new GameObject();
            ga.name = "Language";
            return ga.AddComponent<Lang>();
        }

        protected void readFile()
        {
            _items = new Dictionary<string, string>();

            if (File.Exists(FilePath))
            {
                FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, true);
                while (sr.EndOfStream != true)
                {
                    string[] line = sr.ReadLine().Split(':');
                    if ((line.Length == 2) && !string.IsNullOrEmpty(line[0]))
                        _items[line[0]] = line[1];
                }
                sr.Close();
            }

        }

        public string get(string index)
        {
            if (!string.IsNullOrEmpty(index))
            {
                if (_items.ContainsKey(index))
                    return _items[index];
                else
                {
                    if (index[0] <= 'z')
                        Debug.Log("Lang: expression \"" + index + "\" not found");
                }
            }
            return index;
        }

        public string get(string v, int value)
        {
            return string.Format(get(v), value);
        }

        public string get(string v, string value)
        {
            return string.Format(get(v), value);
        }
    }
}
