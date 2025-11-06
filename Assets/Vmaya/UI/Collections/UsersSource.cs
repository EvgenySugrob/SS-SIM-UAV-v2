using System.Collections.Generic;
using UnityEngine;
using Vmaya.Collections;
using Vmaya.UI.Users;

namespace Vmaya.UI.Collections
{
    public class UsersSource : FileSource
    {
        private UsersData _data;

        public UsersData data { get => _data; }

        protected override void readString(string s_data)
        {
            checkDataInit();
            JsonUtility.FromJsonOverwrite(s_data, _data);
            base.readString(s_data);
        }

        private void checkDataInit()
        {
            if (_data == null)
            {
                _data = new UsersData();
                _data.list = new List<User>();
            }
        }

        protected override string writeData()
        {
            return JsonUtility.ToJson(_data);
        }

        override public int getCount()
        {
            return _data.list.Count;
        }

        override public int IndexOf(string id)
        {
            int iid = int.Parse(id);
            for (int i = 0; i < _data.list.Count; i++)
                if (_data.list[i].id == iid) return i;

            return -1;
        }

        override public string getId(int idx)
        {
            return _data.list[idx].id.ToString();
        }

        override public string getName(int idx)
        {
            return _data.list[idx].name;
        }

        public User Find(string name)
        {
            foreach (User user in _data.list)
                if (user.name.Equals(name)) return user;

            return null;
        }
    }
}