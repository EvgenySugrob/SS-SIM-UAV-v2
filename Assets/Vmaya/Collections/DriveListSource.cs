using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Vmaya.Collections
{
    public class DriveListSource : BaseFSListSource
    {
        public override void Refresh()
        {
            try
            {
                List<FileRecord> result = new List<FileRecord>();
                string[] allDrives = Directory.GetLogicalDrives();
                foreach (string di in allDrives)
                {
                    //di = di.Replace('\\', Path.DirectorySeparatorChar);
                    result.Add(new FileRecord(FSType.Disk, di, di));
                }
                setList(result);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}