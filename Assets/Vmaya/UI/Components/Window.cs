using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems; // Required when using Event data.

namespace Vmaya.UI.Components
{
    public class Window : UIComponent, IPointerDownHandler
    {
        public UnityEvent onShow;
        private Animator _animator => GetComponent<Animator>();

        private static Vector2 _curPos = Vector2.zero;

        [SerializeField]
        private string _showClip = "Window show";
        [SerializeField]
        private string _hideClip = "Window hide";
        protected WindowManager manager => GetComponentInParent<WindowManager>();

        private void Awake()
        {
            init();
        }

        virtual public void init()
        {

        }

        protected virtual void OnEnable()
        {
            doShow();
        }

        virtual protected void doShow()
        {
            onShow.Invoke();
            GetComponent<RectTransform>().SetAsLastSibling();
            if (_animator && !string.IsNullOrEmpty(_showClip) && (Time.timeScale > 0)) _animator.Play(_showClip);
            else gameObject.SetActive(true);
        }

        virtual public void hide()
        {
            if (_animator && !string.IsNullOrEmpty(_hideClip) && (Time.timeScale > 0))
            {
                AnimationClip clip = Vmaya.Utils.GetAnimationClip(_animator, _hideClip);
                _animator.Play(_hideClip);
                Vmaya.Utils.setTimeout(this, () => { gameObject.SetActive(false); }, clip ? clip.length * Time.timeScale : 1f);
            } else gameObject.SetActive(false);
        }

        virtual public void Dispose()
        {
            hide();
            Destroy(gameObject);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            GetComponent<RectTransform>().SetAsLastSibling();
        }

        public void toNextPosition()
        {
            RectTransform crect = transform.parent.GetComponent<RectTransform>();
            RectTransform mrect = GetComponent<RectTransform>();
            if (_curPos.x + mrect.rect.width > crect.rect.width) _curPos.x = 0;
            if (_curPos.y - mrect.rect.height < -crect.rect.height) _curPos.y = 0;
            setPositionView(_curPos);
            _curPos += new Vector2(50, -50);
        }

        public void setPositionView(Vector2 newPos)
        {
            RectTransform prect = transform.parent.GetComponent<RectTransform>();
            RectTransform rect = GetComponent<RectTransform>();
            Vector2 h2 = new Vector2(rect.rect.width * rect.pivot.x, rect.rect.height * rect.pivot.y);

            if (newPos.x + rect.rect.width - h2.x > Screen.width + prect.offsetMax.x)
            {
                if (h2.x < 0) newPos.x = newPos.x - rect.rect.width + h2.x * 2;
                else newPos.x = Screen.width + prect.offsetMax.x - rect.rect.width + h2.x;
            }
            else if (newPos.x - h2.x < prect.offsetMin.x) newPos.x = prect.offsetMin.x + h2.x;

            if (newPos.y + rect.rect.height - h2.y > Screen.height + prect.offsetMax.y) newPos.y = Screen.height + prect.offsetMax.y - rect.rect.height + h2.y;
            else if (newPos.y - h2.y < prect.offsetMin.y) newPos.y = prect.offsetMin.y + h2.y;

            rect.position = newPos;
        }


        public void setPosition(Vector2 pos)
        {
            setPositionView(new Vector2(pos.x * Screen.width, pos.y * Screen.height));
        }

        public void setPositionTransform(Transform trans)
        {
            setPosition(CameraManager.getCurrent().WorldToViewportPoint(trans.position));
        }
    }
}