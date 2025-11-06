using Vmaya.Command;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vmaya.UI.Users;
using Vmaya.Language;
using Vmaya.UI.Components;
using Vmaya.UI.Menu.Command;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vmaya.UI.Menu
{
    public class MenuItem : MonoBehaviour, IMenuItem, IPointerEnterHandler
    {
        [SerializeField]
        protected Button _button;

        [SerializeField]
        protected Component _captionText;

        [SerializeField]
        private Component _shortCutText;

        [SerializeField]
        private SubMenuLayer _subMenuLayer;
        public MenuItem _templateItem;
        private MenuItemRecord _data;
        public MenuItemRecord data => _data;

        public float MinWidth;

        private ShortcutManager.ShortcutData _shortcutData;

        protected MainMenu mainMenu => GetComponentInParent<MainMenu>();
        protected IAvailableProvider availableProvider => mainMenu ? mainMenu.availableProvider : null;

        private bool _interactable = true;
        public bool Interactable { get => _interactable; set => setInteractable(value); }
        protected Selectable[] selectable => GetComponentsInChildren<Selectable>();
        protected BaseMenu topMenu => GetComponentInParent<BaseMenu>();
        protected MenuItem parentMenu => transform.parent.GetComponentInParent<MenuItem>();

        private void OnValidate()
        {
            if (!_button) _button = GetComponent<Button>();
        }

        protected virtual void Start()
        {
            if (_button) _button.onClick.AddListener(OnPointerClick);
        }

        public string CaptionText {
            get => Vmaya.Utils.getText(_captionText);
            set => setCaptionText(value);
        }

        private void setCaptionText(string value)
        {
            Vmaya.Utils.setText(_captionText, value);
        }

        public virtual void setData(User user, MenuItemRecord a_data)
        {
            _data = a_data;

            if (!string.IsNullOrEmpty(_data.action))
            {
                if (!string.IsNullOrEmpty(_data.shortCut))
                {
                    ShortcutManager.parseShortcut(_data.shortCut, ref _shortcutData);
                    if (_shortCutText)
                        Vmaya.Utils.setText(_shortCutText, _data.shortCut);

                } else if (_shortCutText) _shortCutText.gameObject.SetActive(false);

                if (availableProvider != null)
                        setInteractable(availableProvider.GetAvailable(_data.action));
            }

            if (_data is MenuItemData)
            {
                MenuItemData midata = _data as MenuItemData;
                if ((midata.subItems != null) && (midata.subItems.Count > 0) && _subMenuLayer && _templateItem)
                    updateItems(user, _subMenuLayer.transform, midata.subItems, _templateItem);
            }

            Vmaya.Utils.setText(_captionText, Lang.instance[_data.name]);
            setWidth(calcWidth());
        }

        protected void setInteractable(bool value)
        {
            if (_interactable != value) setInteractableA(value);
        }

        protected virtual void setInteractableA(bool value)
        {
            _interactable = value;
            afterChangeInteractable();
        }

        protected virtual void afterChangeInteractable()
        {
            if (selectable.Length > 0)
            {
                foreach (Selectable sel in selectable)
                    sel.interactable = Interactable;

                Vmaya.Utils.setInteractable(_captionText, Interactable);
            }
        }

        public virtual float calcWidth()
        {
            float width = LayoutUtility.GetPreferredWidth(_captionText.GetComponent<RectTransform>());
            if (_shortCutText && _shortCutText.gameObject.activeInHierarchy) width += _shortCutText.GetComponent<RectTransform>().sizeDelta.x;
            return width;
        }

        protected virtual float[] getSpaces()
        {
            RectTransform br = _captionText.GetComponent<RectTransform>();
            return new float[] { br.offsetMin.x, br.offsetMin.x };
        }

        public virtual void setWidth(float a_width)
        {
            RectTransform rt = GetComponent<RectTransform>();

            float[] space = getSpaces();

            if (string.IsNullOrEmpty(_data.name))
                rt.sizeDelta = new Vector2(a_width + space[0] + space[1], 2);
            else
            {
                //RectTransform captionRect = _captionText.GetComponent<RectTransform>();
                //captionRect.sizeDelta = new Vector2(a_width, captionRect.sizeDelta.y);
                rt.sizeDelta = new Vector2(a_width + space[0] + space[1], rt.sizeDelta.y);
            }
        }

        public static void clearChilds(Transform trans)
        {
            if (!Vmaya.Utils.IsDestroyed(trans))
            {
                MenuItem[] children = trans.GetComponentsInChildren<MenuItem>();
                foreach (MenuItem item in children) Destroy(item.gameObject);
            }
        }

        public static void updateItems(User user, Transform parent, List<MenuItemRecord> a_items, MenuItem a_templateItem)
        {
            clearChilds(parent);

            int role = user != null ? user.role : UIPlayer.Role;

            foreach (MenuItemRecord item in a_items)
            {
                if (item.role >= role)
                {
                    MenuItem mitem = Instantiate(a_templateItem, parent);
                    mitem.setData(user, item);
                }
            }
        }

        public static void updateItems(User user, Transform parent, List<MenuItemData> a_items, MenuItem a_templateItem)
        {
            clearChilds(parent);

            int role = user != null ? user.role : UIPlayer.Role;
            int idx = 0;

            foreach (MenuItemData item in a_items)
            {
                if (item.role >= role)
                {
                    idx++;
                    MenuItem mitem = Instantiate(a_templateItem, parent);
                    mitem.name = a_templateItem.name + "-" + idx;
                    mitem.setData(user, item);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_subMenuLayer && isActiveAndEnabled)
            {
                if (!(VMouse.GetMouseButton(0) || VMouse.GetMouseButton(1)))
                    subMenuShowAndRefresh();
            }
        }

        protected void subMenuShowAndRefresh()
        {
            MenuItemData midata = _data as MenuItemData;
            if ((midata != null) && (midata.subItems != null))
            {
                bool isSubMenu = midata.subItems.Count > 0;
                if (isSubMenu)
                {
                    _subMenuLayer.Show();
                    BaseMenu.PrepareItems(_subMenuLayer.GetComponentsInChildren<MenuItem>(), availableProvider, MinWidth);
                }
            }
        }

        protected virtual void callAction()
        {
            topMenu.ActionMap.Call(_data.action);
            topMenu.RefreshAvailable();
        }

        public void execAction()
        {
            if (!CommandManager.isCommandFast)
            {
                if ((topMenu.ActionMap != null) && !string.IsNullOrEmpty(_data.action))
                {
                    callAction();

                    MSCommandManager cm = CommandManager.instance as MSCommandManager;
                    if (cm && cm.isCommandMenuItem(this) && !checkShortCutDown())
                    {
                        if (parentMenu) parentMenu.subMenuShowAndRefresh();
                    }
                    else SubMenuLayer.HideCurrentSubmenu();
                }
                else Debug.Log("Action map not set or action is empty");
            }
        }

        public void OnPointerClick()
        {
            if (Interactable) execAction();
        }
#if ENABLE_INPUT_SYSTEM
        public bool isShortcut => _shortcutData.keyCode != Key.None;
#else
        public bool isShortcut => _shortcutData.keyCode != KeyCode.None;
#endif

        public bool checkShortCutDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (VKeyboard.GetKeyDown(_shortcutData.keyCode))
            {
                if ((!_shortcutData.Ctrl || VKeyboard.GetKey(Key.LeftCtrl)) &&
                    (!_shortcutData.Alt || VKeyboard.GetKey(Key.LeftAlt)) &&
                    (!_shortcutData.Shift || VKeyboard.GetKey(Key.LeftShift)))
                    return true;
            }
#else
            if (VKeyboard.GetKeyDown(_shortcutData.keyCode))
            {
                if ((!_shortcutData.Ctrl || VKeyboard.GetKey(KeyCode.LeftControl)) &&
                    (!_shortcutData.Alt || VKeyboard.GetKey(KeyCode.LeftAlt)) &&
                    (!_shortcutData.Shift || VKeyboard.GetKey(KeyCode.LeftShift)))
                    return true;
            }
#endif
            return false;
        }
    }
}