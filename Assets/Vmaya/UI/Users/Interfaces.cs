using System.Collections.Generic;

namespace Vmaya.UI.Users
{
    [System.Serializable]
    public class User
    {
        public int id;
        public string name;
        public int role;
    }

    [System.Serializable]
    public class UsersData
    {
        public List<User> list;
    }
}