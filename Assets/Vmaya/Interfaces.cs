using System;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya
{

    [System.Serializable]
    public enum TargetFolder { User, App, Main, Persist };
    interface IWindowManager
    {
    }
    public interface IActionCaller
    {
        void Call(string name);
    }

    [System.Serializable]
    public delegate void GlobalComeback(string a_params);

    [System.Serializable]
    public class GlobalEvent
    {
        public string nameEvent;
        public System.Object content;
        public GlobalComeback comeback;

        public GlobalEvent(string a_nameEvent, System.Object a_content)
        {
            nameEvent = a_nameEvent;
            content = a_content;
            comeback = null;
        }
    }

    [System.Serializable]
    public class GlobalEventAction : UnityEvent<GlobalEvent> { }

    interface IAppManager
    {
        public GlobalEventAction OnGlobalEvent();
    }

    public interface IExecutable
    {
        bool getPerformed();
        void Execute();
    }

    public interface IExecutableAndRecoverable: IExecutable
    {
        string getRecoveryData();
        void Recovery(string data);
    }

    public interface IAvailableProvider
    {
        bool GetAvailable(string activity);
    }

    [System.Serializable]
    public class ClipboardData
    {
        public Indent Source;
        public string TypeObject;
        public string RestoreData;

        public ClipboardData(Indent source, string typeObject, string restoreData = "")
        {
            Source = source;
            TypeObject  = typeObject;
            RestoreData = restoreData;
        }
    }

    public interface ICopyable
    {
        ClipboardData Copy();
    }

    public interface IPlacingCopy
    {
        Component Placing(ClipboardData clipboard);
        bool Compatibility(ClipboardData clipboard);
    }

    public interface IDragControl
    {
        bool isDrag();
    }

    [System.Serializable]
    public class Indent
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public string Path;
        [SerializeField]
        public string Type;

        public const char PathSeparator = '/';
        public const char OriginSeparator = '-';

        private Component _component;

        public static bool isNull(Indent indent)
        {
            return (indent == null) || string.IsNullOrEmpty(indent.Name);
        }

        public static void AfterInstance(MonoBehaviour mbh, Indent indent, Action action)
        {
            if (!isNull(indent))
                Utils.PendingCondition(mbh, () =>
                {
                    return Find(indent) != null;
                }, action);
        }

        public static Indent New(Component obj)
        {
            return new Indent(obj);
        }

        public Indent() {}

        public Indent(Component component)
        {
            if (_component = component)
            {
                Name = component.name;
                Path = GetFullPath(component);
                Type = component.GetType().ToString();
            }
        }

        public string Origin => GetOrigin(Name);

        public static T Find<T>(Indent indent) where T : Component
        {
            if (indent != null) return indent.Find<T>();
            return default;
        }

        public static Component Find(Indent indent)
        {
            if (indent != null) return indent.Find();
            return default;
        }

        public T Find<T>() where T: Component
        {
            if (!_component && !isNull(this))
            {
                GameObject ga = GameObject.Find(Path + Name);
                if (ga)
                {
                    T[] list = ga.GetComponents<T>();
                    foreach (T cp in list)
                        if (cp.GetType().ToString().Equals(Type))
                        {
                            _component = cp as Component;
                            break;
                        }
                }
            }

            return (T)(_component ? _component : default);
        }

        public Component Find()
        {
            if (!_component && !isNull(this))
            {
                GameObject ga = GameObject.Find(Path + Name);
                if (ga)
                {
                    Component[] list = ga.GetComponents<Component>();
                    foreach (Component cp in list)
                        if (cp.GetType().ToString().Equals(Type))
                        {
                            _component = cp;
                            break;
                        }
                }
            }

            return _component;
        }

        public string[] PathList()
        {
            if (!isNull(this))
                return Path.Substring(0, Path.Length - 1).Split(PathSeparator);
            else return null;
        }

        public bool Equals(Indent other)
        {
            return Name.Equals(other.Name) && Path.Equals(other.Path) && (Type.Equals(other.Type));
        }

        public static string GetOrigin(string Name)
        {
            string[] list = Name.Split(OriginSeparator, ' ');
            return list[0].Replace("(Clone)", "");
        }

        public static string GetFullPath(Component obj, Transform relative = null)
        {
            Transform trans;

            if (obj == null) return null;

            if (obj is Transform) trans = obj as Transform;
            else trans = obj.transform;

            string path = "";
            while (trans.parent != null)
            {
                trans = trans.parent;
                if (trans == relative) break;
                path = trans.name + PathSeparator + path;
            }


            return path;
        }

        public static int GetScriptNumber(Component obj)
        {
            Component[] list = obj.GetComponents<Component>();
            for (int i = 0; i < list.Length; i++)
                if (list[i] == obj) return i;
            return 0;
        }

        public override string ToString()
        {
            return Path + Name + "#" + Type;
        }

        internal static T FindByInstanceID<T>(int instanceID) where T : UnityEngine.Object
        {
            T[] all = UnityEngine.Object.FindObjectsOfType<T>();
            for (int i = 0; i < all.Length; i++)
                if (all[i].GetInstanceID() == instanceID) return all[i];

            return null;
        }
    }
}