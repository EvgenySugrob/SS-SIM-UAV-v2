using Vmaya.Collections;
using UnityEngine.Events;
using System.IO;

namespace Vmaya.Catalog
{
    abstract public class FileSource : BaseListSource
    {
        public string fileName;
        public UnityEvent onLoaded;

        private bool _isLoaded = false;

        public bool isLoaded
        {
            get
            {
                return _isLoaded;
            }
        }

        private void Start()
        {
            if (fileName.Length > 0) readFromFile(fileName);
        }

        public void readFromFile(string a_fileName)
        {
            FileStream fs = new FileStream(a_fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, true);
            string jsonString = "";
            while (sr.EndOfStream != true)
            {
                jsonString += sr.ReadLine();
            }
            sr.Close();
            readString(jsonString);
        }

        virtual protected void readString(string s_data)
        {
            onLoaded.Invoke();
            onChange.Invoke();
            _isLoaded = true;
        }

        virtual protected string writeData()
        {
            return "";
        }

        public bool saveToFile(string a_fileName)
        {
            if (!a_fileName.Contains(".json")) a_fileName += ".json";

            FileStream fs = new FileStream(a_fileName, File.Exists(a_fileName) ? FileMode.Truncate : FileMode.CreateNew, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            writer.WriteLine(writeData());
            writer.Close();
            fs.Close();
            return true;
        }
    }
}