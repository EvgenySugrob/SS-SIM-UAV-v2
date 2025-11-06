using UnityEngine;
using UnityEngine.UI;


namespace Vmaya.UI.Components
{
    public class Dialog : ModalWindow, IDialog
    {
        [SerializeField]
        private Button okButton;

        private DialogAction _okAction;

        public void Show(DialogAction a_okAction)
        {
            _okAction = a_okAction;
            gameObject.SetActive(true);
        }

        protected virtual void Awake()
        {
            if (okButton) okButton.onClick.AddListener(OnOkClick);
        }

        public virtual void OnOkClick()
        {
            if (_okAction != null) _okAction(this);
            if (manager) manager.CloseWindow(this);
            else hide();
        }
    }
}
