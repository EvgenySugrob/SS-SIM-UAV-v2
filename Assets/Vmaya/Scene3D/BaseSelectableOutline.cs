using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.Scene3D
{
    public class BaseSelectableOutline : SelectableMouse
    {
        [SerializeField]
        private int _color;

        [SerializeField]
        private List<GameObject> exclude;

        override protected void init()
        {
            base.init();
            resetOuline();
        }

        override protected void OnEnable()
        {
            if (!initialize) init();
            else resetOuline();
        }

        private IEnumerator startDisableOutline(Outline outline)
        {
            yield return new WaitForSeconds(0.02f);
            if (!Utils.IsDestroyed(outline)) outline.enabled = SwitchableList.instance[this];
        }

        protected bool isExtrude(GameObject obj)
        {
            return (exclude != null) && exclude.Contains(obj);
        }

        protected virtual void resetOuline()
        {
            if (Application.isPlaying)
            {
                Renderer[] rr = GetComponentsInChildren<Renderer>(true);

                foreach (Renderer r in rr)
                    if (!isExtrude(r.gameObject)) {
                        Outline outline = r.GetComponent<Outline>();
                        if (!outline) outline = r.gameObject.AddComponent<Outline>();
                        if (gameObject.activeInHierarchy) StartCoroutine(startDisableOutline(outline));
                        else outline.enabled = SwitchableList.instance[this];

                    }
            }
        }

        protected virtual void setOutlineVisibility(bool value)
        {
            if (!initialize) init();

            Outline[] items = GetComponentsInChildren<Outline>(true);
            foreach (Outline ouline in items)
                if (ouline.enabled != value) ouline.enabled = value;
        }

        override public void showSelect()
        {
            base.showSelect();
            setOutlineVisibility(true);
        }

        override public void hideSelect()
        {
            base.hideSelect();
            setOutlineVisibility(false);
        }

        public override void Reset()
        {
            resetOuline();
        }

        public override int GetColor()
        {
            return _color;
        }

        public override void SetColor(int idx)
        {
            if (_color != idx)
            {
                _color = idx;
                resetOuline();
            }
        }
    }
}