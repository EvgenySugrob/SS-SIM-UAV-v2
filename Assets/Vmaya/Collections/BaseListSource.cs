using UnityEngine;
using UnityEngine.Events;


namespace Vmaya.Collections
{
    public class BaseListSource : MonoBehaviour, IListSource
    {
        public UnityEvent onChange;

        virtual protected void Awake() { }

        public void Refresh()
        {
            refresh();
        }

        virtual public void onAfterChange(UnityAction complete)
        {
            onChange.AddListener(complete);
        }

        virtual protected void refresh()
        {
            onChange.Invoke();
        }

        virtual public int getCount()
        {
            return 0;
        }

        virtual public int IndexOf(string id)
        {
            return int.Parse(id);
        }

        virtual public string getId(int idx)
        {
            return "";
        }

        virtual public string getName(int idx)
        {
            return "";
        }

        virtual public string getData(int idx)
        {
            return "";
        }
    }
}