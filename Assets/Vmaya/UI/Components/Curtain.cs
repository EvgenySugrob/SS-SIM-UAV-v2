using UnityEngine;
using UnityEngine.UI;
using Vmaya.Util;

namespace Vmaya.UI.Components
{
    public class Curtain : UIComponent, ICurtain
    {
        [SerializeField]
        private RawImage _background;

        [SerializeField]
        [Tooltip("From 0 to 30")]
        private int maxBlur = 5;

        [SerializeField]
        private float _blurSpeed = 0.05f;

        private RenderTexture rt;
        private float _curBlur;
        private float _showDirect;
        private Vector2 _prevSize;

        private static Curtain _instance;

        public static Curtain instance => getInstance();
        internal static bool isModal => instance ? instance.gameObject.activeSelf : false;

        private static Curtain getInstance()
        {
            if (!_instance)
                _instance = FindObjectOfType<Curtain>(true);
            return _instance;
        }

        private void OnValidate()
        {
            if (_background == null) _background = GetComponent<RawImage>();
        }

        private void Awake()
        {
            _instance = this;
            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMin  = Vector2.zero;
            rect.anchorMax  = new Vector2(1, 1);
            rect.pivot      = new Vector2(0.5f, 0.5f);
            rect.localScale = new Vector3(1, 1, 1);
            rect.sizeDelta  = Vector2.zero;
        }

        private void Start()
        {
            if (gameObject.activeInHierarchy)
                setTimeout(updateBackground, 0.1f);
        }

        public void Show()
        {
            updateBackground();
            gameObject.SetActive(true);

            _curBlur = 0;
            _showDirect = _blurSpeed;
        }

        protected void updateBackground()
        {
#if UNITY_EDITOR
            Vector2 ScaleFactor = FixGameViewScale.GetGameWindowScale();
#else
            Vector2 ScaleFactor = new Vector2(1, 1);
#endif

            Vector2 size = GetComponentInParent<RectTransform>().rect.size;
            rt = new RenderTexture((int)(size.x * ScaleFactor.x), (int)(size.y * ScaleFactor.y), 24);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);

            _background.texture = rt;
            _prevSize = size;
        }

        private void updateBlur()
        {
            _background.material.SetFloat("_Size", _curBlur * maxBlur);
        }

        private void Update()
        {
            if (_showDirect > 0)
            {
                if (_curBlur < 1)
                    _curBlur += _showDirect;
                else
                {
                    _curBlur    = 1;
                    _showDirect = 0;
                }
                updateBlur();
            }
            else if (_showDirect < 0)
            {
                if (_curBlur > 0)
                    _curBlur += _showDirect;
                else
                {
                    gameObject.gameObject.SetActive(false);
                    rt.Release();
                    _showDirect = 0;
                    _curBlur    = 0;
                }
                updateBlur();
            }

            Vector2 curSize = GetComponentInParent<RectTransform>().rect.size;
            if (!curSize.Equals(_prevSize)) updateBackground();
        }

        public void Hide()
        {
            _curBlur = 1;
            _showDirect = -_blurSpeed;
        }
    }
}