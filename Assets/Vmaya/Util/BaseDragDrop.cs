using Vmaya.Scene3D;

namespace Vmaya.Util
{
    abstract public class BaseDragDrop : HitMouse, IDragControl
    {
        protected int _drag = 0;
        public static BaseDragDrop CurrentDrag => (hitDetector.Down && (hitDetector.Down is BaseDragDrop) && (hitDetector.Down as BaseDragDrop).isDrag()) ? hitDetector.Down as BaseDragDrop : null;

        public override bool isDrag()
        {
            return _drag == 2;
        }

        public static bool isDragging => CurrentDrag || DragDrop3d.currentDrag;
        public override void doMouseDown()
        {
            base.doMouseDown();
            if (enabled) _drag = 1;
        }

        public override void doMouseUp()
        {
            if (hitDetector.Down == this)
            {
                if (_drag <= 1)
                {
                    if (onClick != null) doClick();
                }
                else Drop();
                _drag = 0;
            }
        }

        override protected void doMouseDrag()
        {
            if (_drag > 0)
            {
                if (Dragging() && (_drag == 1)) beginDrag();

                if (isDrag()) doDrag();
            } 
            else doMouseDown(); // Эта строка для того что бы выбирался объект который начали тащить
        }

        abstract protected bool Dragging();
        abstract protected void doDrag();
        virtual protected void beginDrag() {
            _drag = 2;
        }
        virtual protected void Drop() {}
    }
}