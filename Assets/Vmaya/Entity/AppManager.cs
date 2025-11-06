using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Vmaya.RW;
using UnityEngine.SceneManagement;

namespace Vmaya.Entity
{

    public class AppManager : MonoBehaviour, IAppManager
    {

        [System.Serializable]
        public class SelectedEventAction : UnityEvent<BaseEntity> { }

        private INameProvider _nameProvider;

        [SerializeField]
        private Component nameProvider;
        public INameProvider NameProvider => _nameProvider;

        [SerializeField]
        private Transform Scene;

        private static List<Action> _afterStart = new List<Action>();

        private static AppManager _instance;

        public static AppManager instance => _instance ? _instance : _instance = FindObjectOfType<AppManager>();

        private static bool _isQuit = false;
        public static bool isQuit { get { return _isQuit; } }

        public GlobalEventAction onGlobalEvent;
        private int _currentNameIndex;

        private BaseEntity _currentSelected;
        public BaseEntity currentSelected { get => _currentSelected; }
        [HideInInspector]
        public SelectedEventAction onCurrentSelected;
        public UnityEvent onQuit;
        private bool isStarted = false;
        public static string levelName
        {
            get => SceneManager.GetActiveScene().name;
            set => GLoadScene(value);
        }

        private void OnValidate()
        {
            checkNameProvider();
        }

        protected virtual void Awake()
        {
            if (_instance && (_instance != this)) throw new Exception("Обнаружен дубликат компонента AppManager");
            _currentNameIndex = 0;
        }

        private void checkNameProvider()
        {
            _nameProvider = nameProvider ? nameProvider.GetComponent<INameProvider>() : null;
            nameProvider = _nameProvider as Component;
        }

        protected virtual void Start()
        {
            foreach (Action action in _afterStart) action();
            isStarted = true;
        }

        public static void afterStart(Action action)
        {
            if (!instance) action();
            else if (instance.isStarted) action();
            else _afterStart.Add(action);
        }

        virtual public bool isAllowLibItemDrag()
        {
            return true;
        }

        public virtual void setSelected(BaseEntity entity)
        {
            onCurrentSelected.Invoke(entity);
            _currentSelected = entity;
        }

        private void OnApplicationQuit()
        {
            _isQuit = true;
            onQuit.Invoke();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public static string checkNextName(string checkName)
        {
            if (instance) {
                if (instance.NameProvider != null)
                    return instance.NameProvider.checkNextName(checkName);
                else if (MyData.instance) return MyData.instance.checkNextName(checkName);
                else
                {
                    string[] a = checkName.Split('-');
                    if (a.Length > 1)
                    {
                        int num;
                        int.TryParse(a[a.Length - 1], out num);
                        if (num > 0) return checkName;
                    }

                    instance._currentNameIndex++;
                    return checkName + '-' + instance._currentNameIndex.ToString();
                }
            } else if (MyData.instance) return MyData.instance.checkNextName(checkName);
           
            return checkName;
        }

        public string getPrefix(string fullName)
        {
            string[] list = fullName.Split('-');
            return list[0];
        }

        public void reset()
        {
            _currentNameIndex = 0;
        }

        public void LoadScene(string value)
        {
            GLoadScene(value);
        }

        public static void GLoadScene(string value)
        {
            if (value != levelName)
                SceneManager.LoadScene(value, LoadSceneMode.Single);
        }

        public GlobalEventAction OnGlobalEvent()
        {
            return onGlobalEvent;
        }

        internal static void afterEndOfFrame(Action p)
        {
            Vmaya.Utils.afterEndOfFrame(instance, p);
        }

        internal static void setTimeout(Action p, float sec)
        {
            Vmaya.Utils.setTimeout(instance, p, sec);
        }
    }
}
