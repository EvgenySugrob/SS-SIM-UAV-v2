using UnityEngine;
using UnityEngine.Events;
using Vmaya.Scene3D;
using Vmaya.UI.Menu;

namespace Vmaya.UI.Menu
{
    public class ActionMapTg : ActionMap
    {
        private bool _curValue;
        public void SetValue(Component component)
        {
            ISwitchable a_switch = component.GetComponent<ISwitchable>();
            if (a_switch != null) a_switch.setOn(_curValue);
        }

        public virtual bool callAsToogle(string name, bool value)
        {
            UnityEvent action = findAction(name);
            if (action != null)
            {
                _curValue = value;
                action.Invoke();
                return true;
            }
            return false;
        }
    }
}