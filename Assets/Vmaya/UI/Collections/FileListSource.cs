using Vmaya.RW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vmaya.Collections;
using Vmaya.UI.Components;
using Vmaya.Util;
using MyBox;

namespace Vmaya.UI.Collections
{
    public class FileListSource : BaseFSListSource
    {
        [SerializeField]
        private string _filter;
        [SerializeField]
        private TargetFolder _targetFolder;
        [SerializeField]
        private string _currentPath;
        [SerializeField]
        private bool _showDrives;
        [SerializeField]
        private bool _showDirs;
        [SerializeField]
        private bool _showFiles;
        [SerializeField]
        private bool _keepHomeFolder;
        [SerializeField]
        private bool _createHomeFolder;

        public UnityEvent onChangePath;

        protected string basePath => asPath(PathUtils.getFolder(_targetFolder));
        public string currentPath => _currentPath.Trim().Length > 0 ? _currentPath : GetComponentInParent<MyData>().relativePath;

        private bool _requireRefresh;

        private bool isTop => _keepHomeFolder ? (basePath.Equals(currentPath)) : (_currentPath.Length < 4) && (_currentPath[1] == ':');


        private void Start()
        {
            if (_currentPath == "")
                setAbsolutePath(basePath);
            else setAbsolutePath(MyData.fullDirPath(basePath, _currentPath));
        }

        public void setAbsolutePath(string a_value)
        {
            _currentPath = asPath(a_value);
            Refresh();
            onChangePath.Invoke();
        }

        protected static string asPath(string path)
        {
            if ((path.Length > 1) && (path[path.Length - 1] != Path.DirectorySeparatorChar))
                return path + Path.DirectorySeparatorChar;
            return path;
        }

        public void setRelativePath(ListView drivesList)
        {
            DriveListSource dls = drivesList.Source as DriveListSource;
            if (dls) setAbsolutePath(dls[drivesList.selectedIndex].name);
        }

        public void appendRelative(ListView a_list)
        {
            if (a_list.Source == this as IListSource)
            {
                string sel = this[a_list.selectedIndex].name;

                if (this[a_list.selectedIndex].type == FSType.File)
                    return;

                if (this[a_list.selectedIndex].type == FSType.Dir)
                {
                    if (sel == "..")
                    {
                        if (_currentPath.Length > 2)
                        {
                            int lix = _currentPath.LastIndexOf(Path.DirectorySeparatorChar, _currentPath.Length - 2);
                            if (lix > -1) _currentPath = _currentPath.Substring(0, lix) + Path.DirectorySeparatorChar;
                        }
                    }
                    else _currentPath = asPath(_currentPath) + asPath(this[a_list.selectedIndex].name);
                }
                else if (this[a_list.selectedIndex].type == FSType.Disk)
                {
                    _currentPath = sel;
                }

                Refresh();
                onChangePath.Invoke();
            }
        }

        protected void refreshLists()
        {
            List<FileRecord> result = new List<FileRecord>();
            try
            {
                if (_createHomeFolder) PathUtils.checkAndCreateDirectory(currentPath);

                DirectoryInfo dir = new DirectoryInfo(currentPath);
                DirectoryInfo[] dirs = dir.GetDirectories();
                FileInfo[] files = dir.GetFiles();

                if (!isTop) result.Add(new FileRecord(FSType.Dir, "..", currentPath));
                else if (_showDrives)
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo di in allDrives)
                    {
                        string dirstr = di.RootDirectory.Name.Replace('\\', Path.DirectorySeparatorChar);
                        result.Add(new FileRecord(FSType.Disk, dirstr, dirstr));
                    }
                }

                if (_showDirs)
                    foreach (DirectoryInfo d in dirs)
                        result.Add(new FileRecord(FSType.Dir, d.Name, d.FullName));

                if (_showFiles)
                    foreach (FileInfo f in files)
                        if (!(((f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) ||
                                ((f.Attributes & FileAttributes.System) == FileAttributes.System)) && Filter(f))
                            result.Add(new FileRecord(FSType.File, f.Name, f.FullName));

            }
            catch (Exception)
            {
                Debug.Log(currentPath);

                if (!isTop)
                    result.Add(new FileRecord(FSType.Dir, "..", currentPath));
            }
            setList(result);
        }

        protected IEnumerator delayRefresh()
        {
            yield return new WaitForSeconds(0.01f);
            refreshLists();
        }

        protected virtual bool Filter(FileInfo f)
        {
            if (!string.IsNullOrEmpty(_filter))
                return _filter.Contains(f.Extension);
            else return true;
        }

        override public void Refresh()
        {
            _requireRefresh = true;
            //StartCoroutine(delayRefresh());
        }

        private void LateUpdate()
        {
            if (_requireRefresh)
            {
                _requireRefresh = false;
                refreshLists();
            }
        }

        public void outCurrentPath(Component output)
        {
            Vmaya.Utils.setText(output, _currentPath);
        }
    }
}