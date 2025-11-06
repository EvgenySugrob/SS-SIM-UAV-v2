using System.Collections.Generic;

namespace Vmaya.UI
{
    public interface ICursorManager
    {
        public void setCursor(string nameCursor);
    }

    public interface ITextField
    {
        public string text { get; set; }
    }

    [System.Serializable]
    public delegate void DialogAction(IDialog a_dialog);

    public interface IDialog
    {
        void Show(DialogAction okAction);
    }

    public interface ISelectedList
    {
        List<string> getSelected();
        List<string> getSelectedValues();
    }

    public interface ICurtain
    {
        void Show();
        void Hide();
    }
}