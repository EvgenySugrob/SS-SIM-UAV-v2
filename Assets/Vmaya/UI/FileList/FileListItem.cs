using Vmaya.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.UI.Components;

namespace Vmaya.UI.FileList
{
    public class FileListItem : ListViewItem
    {
        [SerializeField]
        private Image _dirImage;
        [SerializeField]
        private Image _diskImage;
        [SerializeField]
        private Image _fileImage;

        protected BaseFSListSource source
        {
            get
            {
                return listView.Source as BaseFSListSource;
            }
        }

        override public void setData(string a_id, string a_caption)
        {
            base.setData(a_id, a_caption);
            _diskImage.gameObject.SetActive(source[source.IndexOf(id)].type == FSType.Disk);
            _dirImage.gameObject.SetActive(source[source.IndexOf(id)].type == FSType.Dir);
            _fileImage.gameObject.SetActive(source[source.IndexOf(id)].type == FSType.File);
        }
    }
}