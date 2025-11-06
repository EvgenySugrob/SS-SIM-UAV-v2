using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vmaya.Command;
using Vmaya.Entity;
using Vmaya.Language;
using Vmaya.RW;
using Vmaya.UI.Collections;
using Vmaya.UI.Components;
using static Vmaya.RW.RWEvents;

namespace Vmaya.UI.Users
{
    public class UIPlayer : MonoBehaviour, IRW, IOpener
    {
        [System.Serializable]
        public class loginEvent : UnityEvent<User> { }

        [System.Serializable]
        public class PlayerSettings
        {
            public string sceneName;
        }

        public string basePath;
        [SerializeField]
        private int minLengthLogin = 5;
        [SerializeField]
        protected MyData GeneralData;
        [SerializeField]
        protected MyData SettingsData;
        [SerializeField]
        private UsersSource _usersSource;
        [SerializeField]
        private ModalWindow _loginDialog;
        [SerializeField]
        protected InputField _fileNameField;
        [SerializeField]
        private Text _loginCaption;
        [SerializeField]
        private bool devUser;
        [SerializeField]
        private int devRole;
        public string DefaultUser;
        [SerializeField]
        public string DefaultPass;
        [SerializeField]
        public string _newFileName;
        [SerializeField]
        [Range(0, 128)]
        public int DefaultRole;

        private User _user;
        public User User => _user;
        public int roleId => _user != null ? _user.role : DefaultRole;

        public static int Role => UIPlayer.instance ? UIPlayer.instance.roleId : 0;

        private static string _userDirName;

        public static bool initialized => _instance._user != null;

        public UnityEvent onClearProject;
        private bool _unionMode;

        private static UIPlayer _instance;
        public static UIPlayer instance => _instance;
        public loginEvent onAfterLogin;
        public string NameCurrentProject => _fileNameField ? _fileNameField.text : null;
        private bool isDevUser => Application.isEditor && devUser;

        private int _pointCommands = -1;

        protected virtual void Awake()
        {
            _instance = this;
            if (SettingsData) SettingsData.checkFolders(basePath);

            if (GeneralData)
            {
                GeneralData.checkFolders(basePath);
                GeneralData.onLoad.AddListener(afterOpenFile);
            }
            if (_fileNameField)
            {
                _fileNameField.text = _newFileName;
                GeneralData.onChangeFilename.AddListener(onChangeFilename);
            }
        }

        private void onChangeFilename()
        {
            string fileName = GeneralData.fileName;
            int idx = fileName.IndexOf(".json");
            _fileNameField.text = idx > 0 ? fileName.Substring(0, fileName.IndexOf(".json")) : fileName;
            CheckAndSaveSettings();
        }

        private void Start()
        {
            if (SettingsData) SettingsData.addListener(this);

            if (initialized && _loginDialog)
            {
                _loginDialog.hide();
                initUser(_user);
            } else if (isDevUser)
            {
                if (_loginDialog) _loginDialog.hide();
                _userDirName = "dev\\";
                User user = new User();
                user.id = 0;
                user.name = "Dev";
                user.role = devRole;
                initUser(user);
            }
        }

        public void afterLogin(UnityAction<User> action)
        {
            if (User != null) action(User);
            else onAfterLogin.AddListener(action);
        }

        public void setUnionMode(bool value)
        {
            _unionMode = value;
        }

        public static string Md5Sum(string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');

            return hashString.PadLeft(32, '0');
        }

        protected string baseFolder()
        {
            return GeneralData ? GeneralData.BaseFolder : "";
        }

        private bool checkLogin(string name, string passw)
        {
            if ((name.Length >= minLengthLogin) && (passw.Length >= minLengthLogin))
            {
                User user;
                string userName = name;
                _userDirName = Md5Sum(name + passw) + "\\";
                //Debug.Log("User dir: " + dirName);

                user = _usersSource.Find(userName);
                string fullUserPath = MyData.fullDirPath(baseFolder(), userRelativePath());

                if ((user == null) || !Directory.Exists(fullUserPath))
                {
                    Debug.Log("Dir: " + userRelativePath());
                    //Question.Show("Папка пользователя не найдена. Cоздать её?", null, true, onCreateUser);
                    Question.Show("Пользователь не найден. Создать нового пользователя с набором прав " + DefaultRole + "?", null, true, () => {
                        if (user == null)
                        {
                            user = new User();
                            user.id = _usersSource.data.list.Count;
                            user.name = name;
                            user.role = DefaultRole;

                            _usersSource.data.list.Add(user);
                            _usersSource.saveToFile(_usersSource.fileName);
                        }

                        Directory.CreateDirectory(MyData.fullDirPath(baseFolder(), userRelativePath()));

                        checkLogin(name, passw);
                    });
                    return false;
                }
                else
                {
                    initUser(user);
                    return true;
                }
            }
            else Question.Show("Логин или пароль должен быть не менее " + minLengthLogin + " символов", null, false, null);
            return false;
        }

        private void initUser(User user)
        {
            _user = user;

            if (_loginCaption) _loginCaption.text = _user.name;

            if (GeneralData)
                GeneralData.relativePath = userRelativePath();

            openSettings();
            onAfterLogin.Invoke(user);

            Debug.Log(_user.name + " user login with " + _user.role + " rights");
        }

        private void openSettings()
        {
            if (SettingsData)
            {
                string fullPathFileName = MyData.fullFilePath(SettingsData.BaseFolder, userRelativePath(), "settings.json");

                SettingsData.relativePath = userRelativePath();
                SettingsData.fileName = "settings.json";

                if (!File.Exists(fullPathFileName))
                    CheckAndSaveSettings();

                SettingsData.Open();
            }
        }

        public virtual bool isChangeSetting()
        {
            return true;
        }

        public virtual bool isChangeGeneral()
        {
            return _pointCommands != CommandManager.instance.pointer;
        }

        public void CheckAndSaveSettings()
        {
            if (SettingsData && isChangeSetting()) 
                SettingsData.Save();
        }

        public void CheckAndSaveGeneral(Action afterCheck)
        {
            if (GeneralData && isChangeGeneral())
            {
                Question.Show(Lang.instance["Save changes?"], (bool result) =>
                {
                    if (result)
                    {
                        if (!string.IsNullOrEmpty(GeneralData.fileName))
                        {
                            GeneralData.Save();
                            afterCheck();
                        }
                        else SaveAs(afterCheck);
                    } else afterCheck();
                });
            }
            else afterCheck();
        }

        protected void openFileNameA(string fileName)
        {
            if (File.Exists(fileName))
            {

                FileInfo fi = new FileInfo(fileName);

                ClearProject();
                GeneralData.Open(fi.DirectoryName + Path.DirectorySeparatorChar, fi.Name);
            }
        }

        protected virtual void afterOpenFile()
        {
            rightToast.setText(Lang.instance["File opened"] + " " + GeneralData.fileName);

            Vmaya.Utils.setTimeout(this, () =>
            {
                _pointCommands = CommandManager.instance ? CommandManager.instance.pointer : 0;
            }, 1);
        }

        private IEnumerator tryOpenFileName(string fileName)
        {
            yield return new WaitForSeconds(0.1f);
            openFileNameA(fileName);
        }

        public void openFileName(string filePath, bool union)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!union)
                {
                    ClearProject();
                    StartCoroutine(tryOpenFileName(filePath));
                }
                else openFileNameA(filePath);
            }
        }

        public bool login(string name, string passw)
        {

            if (checkLogin(name, passw))
            {
                return true;
            }

            return false;
        }

        protected string userRelativePath()
        {
            return basePath + _userDirName;
        }

        public void Exit()
        {
            CheckAndSaveGeneral(()=>
            {
                CheckAndSaveSettings();
                Application.Quit();
            });
        }

        public virtual bool readRecord(RWEvents.dataRecord rec)
        {
            if (rec.indent != null)
            {
                PlayerSettings ps = JsonUtility.FromJson<PlayerSettings>(rec.data);
                return true;
            }

            return false;
        }

        public virtual string writeData()
        {
            PlayerSettings ps = new PlayerSettings();
            ps.sceneName = AppManager.levelName;
            return JsonUtility.ToJson(ps);
        }

        public Indent getIndent()
        {
            return Indent.New(this);
        }

        public RWEvents.dataRecord writeRecord()
        {
            dataRecord dr = new dataRecord();
            dr.indent = getIndent();
            dr.data = writeData();

            return dr;
        }

        public void SaveAs()
        {
            SaveAs(null);
        }

        public void SaveAs(Action afterSave)
        {
            string fileName = _fileNameField.text.Trim();
            if ((fileName.Length > 0) && !fileName.Equals(_newFileName))
            {
                if (!fileName.Contains(".json")) fileName += ".json";

                GeneralData.relativePath = userRelativePath();
                GeneralData.fileName = fileName;

                GeneralData.Save();
                if (afterSave != null) afterSave();
            }
            else {
                Window w = _fileNameField.gameObject.GetComponentInParent<Window>(true);
                if (w) w.gameObject.SetActive(true);
            }
        }

        public void ClearProject()
        {
            onClearProject.Invoke();
            GeneralData.fileName = "";
            GeneralData.relativePath = "";
            CheckAndSaveSettings();
        }

        public void Open(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    openFileName(fileName, _unionMode);
                } catch
                {
                    Debug.Log(Lang.instance["Unknown error"]);
                }
            } else
            {
                Debug.Log(Lang.instance.get("File {0} not found", fileName));
            }
        }
    }
}