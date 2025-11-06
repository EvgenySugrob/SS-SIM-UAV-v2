using System;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.Command;

namespace Vmaya.UI.Menu.Command
{
    public class MSCommandManager : CommandManager
    {
        [SerializeField]
        private int _limitSymbols = 20;
        public virtual void resetMenuItem(MenuItem item)
        {
            if (item.data.action.Equals("Undo"))
            {
                item.CaptionText = item.data.name +
                    ((pointer >= 0) ? " " + Vmaya.Utils.WithMaxLength(pointerNameCommand, _limitSymbols) : "");
            }
            else if (item.data.action.Equals("Redo"))
            {
                bool isInteractable = (getCount() > 0) && (pointer < getCount() - 2);

                item.CaptionText = item.data.name +
                    (isInteractable ? " " + Vmaya.Utils.WithMaxLength(this[pointer + 1].commandName(), _limitSymbols) : "");
            }
        }

        internal bool isCommandMenuItem(MenuItem item)
        {
            return item.data.action.Equals("Undo") || item.data.action.Equals("Redo");
        }
    }
}
