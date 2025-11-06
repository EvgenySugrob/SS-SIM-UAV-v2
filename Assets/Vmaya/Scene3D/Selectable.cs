using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Vmaya.Scene3D
{
    public abstract class Selectable : MonoBehaviour, ISelectable
    {

        private OnChangeSelectedEvent _listeners = new OnChangeSelectedEvent();

        private bool _visible;

        public bool Visible => _visible;

        public string adjustersText => SwitchableList.instance != null ? SwitchableList.instance[this].ToString() : "";

        public abstract void Reset();

        virtual public void showSelect()
        {
            _visible = true;
            _listeners.Invoke(true);
        }

        virtual public void hideSelect()
        {
            _visible = false;
            _listeners.Invoke(false);
        }

        public virtual bool isFocus()
        {
            return hitDetector.instance.getFocus() == GetComponent<baseHitMouse>();
        }

        public void removeListener(UnityAction<bool> listener)
        {
            _listeners.RemoveListener(listener);
        }

        public void addListener(UnityAction<bool> listener)
        {
            _listeners.AddListener(listener);
        }

        public virtual void setOn(bool value)
        {
            if (value) showSelect();
            else hideSelect();
        }

        public virtual int GetColor()
        {
            return 0;
        }

        public virtual void SetColor(int idx)
        {
        }

        /*
        internal void setEnable(Object initiator, bool v)
        {
            SwitchableList.AddSwitchable(this as ISwitchable, initiator, v);
        }
        */
    }
}
