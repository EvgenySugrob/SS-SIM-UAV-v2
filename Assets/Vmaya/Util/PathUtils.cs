using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Vmaya.Util
{
    public class PathUtils
    {
        public static string appFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + Application.productName + Path.DirectorySeparatorChar;
        }

        public static string userFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + Application.productName + Path.DirectorySeparatorChar;
        }

        public static string persistFolder()
        {
            return Application.persistentDataPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public static string mainFolder()
        {
            return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
        }

        public static string getFolder(TargetFolder target)
        {
            switch (target)
            {
                case TargetFolder.App: return appFolder();
                case TargetFolder.User: return userFolder();
                case TargetFolder.Main: return mainFolder();
                case TargetFolder.Persist: return persistFolder();
            }
            return null;
        }

        internal static void checkAndCreateDirectory(string fullPathDirectory)
        {
            FileInfo fi = new FileInfo(fullPathDirectory);
            if (!string.IsNullOrEmpty(fi.DirectoryName) && !Directory.Exists(fi.DirectoryName))
                Directory.CreateDirectory(fi.DirectoryName);
        }

        public static string PrepareFilePath(string fullFilePath)
        {
            string fileFolder = Path.GetDirectoryName(fullFilePath);
            if (!Directory.Exists(fileFolder)) Directory.CreateDirectory(fileFolder);
            return fullFilePath;
        }

        public static bool isAbsolutePath(string path)
        {
            string s = path.Substring(1, 2);
            return s.Equals(":/") || s.Equals(":\\");
        }
    }
}
