using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.Scene3D
{
    public class HitMouse : baseHitMouse
    {
        [System.Serializable]
        public class HMEvent : UnityEvent<baseHitMouse>
        {
        }

        [HideInInspector]
        public HMEvent onRollOver   = new HMEvent();
        [HideInInspector]
        public HMEvent onRollOut    = new HMEvent();
        public HMEvent onMouseDown  = new HMEvent();
        public HMEvent onClick      = new HMEvent();

        private bool _hit;
        public bool isHit => _hit;

        private static HitMouse _focus;
        public static HitMouse focus => _focus;
        public bool isFocus { get { return focus ? focus.transform.IsChildOf(transform) : false; } }

        override public void doOver()
        {
            _hit = true;
            _focus = this;
            if (onRollOver != null) onRollOver.Invoke(this);
            base.doOver();
        }

        override public void doOut()
        {
            _hit = false;
            if (_focus == this) _focus = null;
            if (onRollOut != null) onRollOut.Invoke(this);
            base.doOut();
        }

        public override void doMouseDown()
        {
            base.doMouseDown();
            if (onMouseDown != null) onMouseDown.Invoke(this);
        }

        override public void doClick()
        {
            if (onClick != null) onClick.Invoke(this);
        }
    }
}
