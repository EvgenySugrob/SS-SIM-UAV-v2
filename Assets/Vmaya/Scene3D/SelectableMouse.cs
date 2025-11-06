using System;
using UnityEngine;
using Vmaya.Util;

namespace Vmaya.Scene3D
{
    public abstract class SelectableMouse : Selectable
    {

        private bool _initialize = false;
        protected bool initialize => _initialize;

        private static SelectableMouse _current;
        public static SelectableMouse Current => _current;

        virtual protected void init()
        {
            _initialize = true;
        }

        private void Start()
        {
            hitDetector.instance.onUp.AddListener(onUp); 
            
            HitMouse[] hmList = GetComponents<HitMouse>();

            if (hmList.Length == 0)
                Debug.Log("You may not have installed the component `HitMouse` to " + name);
            else
            {
                foreach (HitMouse hm in hmList)
                {
                    hm.onRollOver.AddListener(onOver);
                    hm.onRollOut.AddListener(onOut);
                }
            }
        }

        private void onUp(baseHitMouse hit)
        {
            if (!hit)
                setCurrent(null);
        }

        private static void setCurrent(SelectableMouse value)
        {
            if (_current != value)
            {
                if (_current) SwitchableList.AddSwitchable(_current, _current, false);
                _current = value;
                if (_current) SwitchableList.AddSwitchable(_current, _current, true);
            }
        }

        virtual protected void OnEnable()
        {
            if (!_initialize) init();
        }

        protected virtual void onOut(baseHitMouse hm)
        {
            if (isActiveAndEnabled && !BaseDragDrop.isDragging) 
                setCurrent(null);
        }

        protected virtual void onOver(baseHitMouse hm)
        {
            if (isActiveAndEnabled && !BaseDragDrop.isDragging)
                setCurrent(this);
        }
    }
}