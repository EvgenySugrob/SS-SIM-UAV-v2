using UnityEngine.Events;

namespace Vmaya.Collections
{
    public interface IListSource
    {
        int getCount();
        int IndexOf(string id);
        string getId(int idx);
        string getName(int idx);
        string getData(int idx);
        void onAfterChange(UnityAction onChange);
        void Refresh();
    }

    public interface IListFilter
    {
        bool Contains(string itemId);
    }
}