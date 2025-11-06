using System;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.Collections;
using Vmaya.RW;
using Vmaya.UI.Components;
using static Vmaya.Collections.BaseFSListSource;

namespace Vmaya.UI.FileList
{
    public class FileListDialog : BaseFileListDialog
    {
        private IOpener _opener;

        protected override void doSelectFile(string fullPathFile)
        {
            base.doSelectFile(fullPathFile);
            if (_opener != null) _opener.Open(fullPathFile);
        }

        public void Open(Transform openerTransform)
        {
            IOpener a_opener = openerTransform.GetComponent<IOpener>();
            if (a_opener != null)
            {
                _opener = a_opener;
                gameObject.SetActive(true);
            }
        }
    }
}
