using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.Collections;
using Vmaya.UI.Components;
using static Vmaya.Collections.BaseFSListSource;

namespace Vmaya.UI.FileList
{
    public abstract class BaseFileListDialog : ModalWindow
    {
        [SerializeField]
        private Button _okButton;
        protected Button OkButton => _okButton;

        [SerializeField]
        private FileListView _fileListView;
        protected FileListView FileListView => _fileListView;
        public FileRecord selectedFileRecord => FileListView.FileListSource[FileListView.selectedIndex];

        public SelectedEvent onSelectedFile;

        protected virtual void Awake()
        {
            _okButton.interactable = false;
            _fileListView.onSelectItem.AddListener(onSelectItem);
            _fileListView.onDblClick.AddListener(onDblClick);
            _okButton.onClick.AddListener(onOkButton);
        }

        protected virtual void onOkButton()
        {
            onSelectFile();
        }

        protected virtual void onDblClick()
        {
            onSelectFile();
        }

        protected void onSelectFile()
        {
            FileRecord fr = selectedFileRecord;
            if (OkButton.interactable = fr.type == FSType.File)
            {
                doSelectFile(fr.path);
                onSelectedFile.Invoke(fr.path);
            }
        }

        protected virtual void doSelectFile(string fullPathFile)
        {
            hide();
        }

        protected virtual void onSelectItem(string id)
        {
            _okButton.interactable = selectedFileRecord.type == FSType.File;
        }
    }
}
