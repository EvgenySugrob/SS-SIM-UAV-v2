using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vmaya.Entity;
using Vmaya.Util;

namespace Vmaya.RW
{
    public class MyData : RWManager, IWriter, IOpener
    {

        public TargetFolder targetFolder;

        public string relativePath;
        public string startFileName;
        public string version;
        public bool stopPhysics = false;

        public bool useCompression;

        private static bool _lastAutoSimulation;

        private static List<MyData> _startLoadList = new List<MyData>();

        public string fileName
        {
            set
            {
                if (_fileName != value)
                {
                    if (!string.IsNullOrEmpty(value) && value.Contains(":" + Path.DirectorySeparatorChar))
                    {
                        FileInfo fi = new FileInfo(value);
                        if (!string.IsNullOrEmpty(fi.DirectoryName))
                        {
                            _targetFolder = fi.DirectoryName + Path.DirectorySeparatorChar;
                            relativePath = "";
                        }

                        _fileName = fi.Name;
                    }
                    else _fileName = value;

                    onChangeFilename.Invoke();
                }
            }

            get => _fileName;
        }

        private string _targetFolder;

        public string BaseFolder => _targetFolder;
        private string _fileName;
        public UnityEvent onChangeFilename;
        private float started;

        public UnityEvent onLoad;
        public UnityEvent onBeforeOpen;

        [System.Serializable]
        public class OnSaveEvent : UnityEvent<string> {}
        public OnSaveEvent onAfterSave;

        private int onLoadInd = 0;

        private static MyData _instance;
        public static MyData instance { get { return _instance; } }

        override protected void Awake()
        {
            _instance = this;
            base.Awake();
            checkFolders(relativePath);
            started = Time.fixedTime + 0.01f;
        }


        private static int _lastFrameRate;
        private string _jsonData;
        public string JsonData => _jsonData;

        public static void FrameRatePause()
        {
            if (_lastFrameRate == 0)
            {
                _lastFrameRate = Application.targetFrameRate;
                Application.targetFrameRate = 0;
            }
        }

        public static void FrameRateContinue()
        {
            if (_lastFrameRate > 0)
            {
                Application.targetFrameRate = _lastFrameRate;
                _lastFrameRate = 0;
            }
        }

        public void checkFolders(string relativePath)
        {
            _targetFolder = PathUtils.getFolder(targetFolder);

            if (!string.IsNullOrEmpty(_targetFolder) && (_targetFolder[_targetFolder.Length - 1] != Path.DirectorySeparatorChar))
                _targetFolder += Path.DirectorySeparatorChar;

            if (!Directory.Exists(_targetFolder))
                Directory.CreateDirectory(_targetFolder);

            
            if (!string.IsNullOrEmpty(relativePath))
            {
                if (!Directory.Exists(_targetFolder + relativePath))
                    Directory.CreateDirectory(_targetFolder + relativePath);
            }
        }

        private void startData()
        {
            if (startFileName.Length > 0) this.Open(startFileName);
        }

        private void Update()
        {
            if ((started > 0) && (started < Time.fixedTime))
            {
                started = 0;
                startData();
            }

            if (onLoadInd > 0)
            {
                onLoadInd--;
                if (onLoadInd == 0)
                    doLoad();
            }
        }

        private void doLoad()
        {
            _startLoadList.Remove(this);

            if ((_startLoadList.Count == 0) && stopPhysics)
            {
                Utils.afterEndOfFrame(this, () =>
                {
                    FrameRateContinue();
                    Physics.autoSimulation = _lastAutoSimulation;
                });
            }

            onLoad.Invoke();
        }

        public bool Open()
        {
            return readFromFile(fullFilePath(BaseFolder, relativePath, _fileName));
        }

        public void Open(string a_path, string a_fileName)
        {
            relativePath = a_path;
            Open(a_fileName);
        }

        public void Open(string fullFilePath)
        {
            fileName = fullFilePath;
            onBeforeOpen.Invoke();
            StartCoroutine(OpenDelay());
            //Open();
        }

        private IEnumerator OpenDelay()
        {
            yield return new WaitForEndOfFrame();
            Open();
        }

        public void Save(InputField fileNameInput)
        {
            fileName = fileNameInput.text;
            Save();
        }

        public void Save(string fullPathToFile)
        {
            fileName = fullPathToFile;
            Save();
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(_fileName))
                saveToFile(fullFilePath(BaseFolder, relativePath, _fileName));
        }

        protected virtual bool checkVersion(ref string jsonData)
        {
            if (!string.IsNullOrEmpty(version))
            {
                if (version.Equals(jsonData.Substring(0, version.Length))) {
                    jsonData = jsonData.Remove(0, version.Length);
                } else return false;
            }
            return true;
        }

        private bool readFromFile(string a_fileName)
        {
            bool result = false;
            if (File.Exists(a_fileName))
            {
                _jsonData = File.ReadAllText(a_fileName);
                if (checkVersion(ref _jsonData))
                {
                    if (useCompression) _jsonData = JsonUtils.Unpack(_jsonData);

                    if (stopPhysics)
                    {
                        if (_startLoadList.Count == 0)
                        {
                            _lastAutoSimulation = Physics.autoSimulation;
                            Physics.autoSimulation = false;

                            FrameRatePause();
                        }

                        _startLoadList.Add(this);
                    }

                    if (result = readString(_jsonData))
                        onLoadInd = 2;
                }
                else VersionWrong();
            }
            else
            {
                Debug.Log("File " + a_fileName + " not found");
                onLoadInd = 2;
            }
            return result;
        }

        protected virtual void VersionWrong()
        {
            AppManager.instance.onGlobalEvent.Invoke(new GlobalEvent("RIGHT_TOASH", "Wrong file version"));
            Debug.Log("Wrong file version");
            onLoadInd = 2;
        }

        public static string fullFilePath(string basePath, string relativePath, string fileName)
        {
            string result;
            if ((fileName.Length > 3) && fileName[1].Equals(':')) result = fileName;
            else
            {
                if (!string.IsNullOrEmpty(relativePath))
                    result = relativePath + (!string.IsNullOrEmpty(fileName) && !relativePath[relativePath.Length - 1].Equals(Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : "") + fileName;
                else result = fileName;
            }

            if (!result[1].Equals(':'))
            {
                result = basePath + result;
            }

            return result;
        }

        public static string fullDirPath(string basePath, string relativePath)
        {
            string result;
            if ((relativePath.Length > 3) && (relativePath[1].Equals(':'))) result = relativePath;
            else
            {
                if (!string.IsNullOrEmpty(relativePath))
                    result = relativePath + (!relativePath[relativePath.Length - 1].Equals(Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : "");
                else result = relativePath;
            }

            if (!result[1].Equals(':')) result = basePath + result;

            return result;
        }

        private bool saveToFile(string a_fileName)
        {
            if (!a_fileName.Contains(".json")) a_fileName += ".json";
            string jsonData = writeData();
            if (useCompression) jsonData = JsonUtils.PackJson(jsonData);
            File.WriteAllText(a_fileName, version + jsonData);
            onAfterSave.Invoke(a_fileName);
            return true;
        }
    }
}