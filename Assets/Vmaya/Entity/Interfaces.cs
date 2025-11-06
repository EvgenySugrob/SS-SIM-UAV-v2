using UnityEngine;

namespace Vmaya.Entity
{
    [System.Serializable]
    public class restoreData
    {
        [SerializeField]
        public bool active;
    }

    public interface ICloneable
    {
        ICloneable Clone();
    }

    public interface IJsonObject
    {
        void setJson(string jsonData);
        string getJson();
        void setActive(bool value);
        bool getActive();
    }

    public interface IRestored: ICloneable
    {
        void Restore(restoreData data);
        string getRestoreDataJson();
        restoreData JsonToRestoreData(string jsonData);
    }

    public interface IRemovable: IRestored
    {
        bool possibleRemove();
        void Delete();
        void Destroy(restoreData data = null);
    }
}
