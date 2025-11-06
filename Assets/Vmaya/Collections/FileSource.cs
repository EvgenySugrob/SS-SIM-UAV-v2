using UnityEngine.Events;
using System.IO;


namespace Vmaya.Collections
{
    public class FileSource : BaseListSource
    {
        public string fileName;
        public UnityEvent onLoaded;

        protected virtual void Start()
        {
            if (!string.IsNullOrEmpty(fileName)) readFromFile(fileName);
        }

        public void readFromFile(string a_fileName)
        {
            fileName = a_fileName;
            readString(File.ReadAllText(a_fileName));
        }

        virtual protected void readString(string s_data)
        {
            onLoaded.Invoke();
            onChange.Invoke();
        }

        virtual protected string writeData()
        {
            return "";
        }

        public bool saveToFile(string a_fileName)
        {
            if (!a_fileName.Contains(".json")) a_fileName += ".json";
            File.WriteAllText(a_fileName, writeData());
            return true;
        }
    }
}