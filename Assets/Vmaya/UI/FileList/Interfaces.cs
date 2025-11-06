using UnityEngine.Events;

namespace Vmaya.UI.FileList
{
    [System.Serializable]
    public delegate void OnPreserver(string fullFilePath);

    [System.Serializable]
    public class SelectedEvent : UnityEvent<string> {}
}