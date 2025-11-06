using UnityEngine;
using Vmaya.Collections;
using Vmaya.UI.Collections;
using Vmaya.UI.Components;
using static Vmaya.Collections.BaseFSListSource;

namespace Vmaya.UI.FileList
{
    public class FileListView : ListView
    {
        public ListViewItem.OnSelectItem onSelectFile;
        public FileListSource FileListSource => Source as FileListSource;

        protected override IListSource checkSource(Component value)
        {
            value = (value != null) ? value.GetComponent<FileListSource>() : null;
            return (value != null) ? value.GetComponent<IListSource>() : null;
        }

        protected override void OnSelectItem(string id)
        {
            base.OnSelectItem(id);
            if (FileListSource[selectedIndex].type == FSType.File) OnSelectFile(FileListSource[selectedIndex]);
            else FileListSource.appendRelative(this); 
        }

        protected virtual void OnSelectFile(FileRecord fileRecord)
        {
            onSelectFile.Invoke(fileRecord.path);
        }
    }
}
