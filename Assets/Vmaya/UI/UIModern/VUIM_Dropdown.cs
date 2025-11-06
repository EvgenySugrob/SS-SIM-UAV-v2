using TMPro;
using UnityEngine;

namespace Vmaya.UI.UIModern
{
    public class VUIM_Dropdown : TMP_Dropdown
    {
        protected override void DestroyBlocker(GameObject blocker)
        {
            RectTransformAnim rta = GetComponentInChildren<RectTransformAnim>();

            float delay = alphaFadeSpeed;
            if (rta) delay = rta.Duration;
            Vmaya.Utils.setTimeout(this, () => { baseDestroyBlocker(blocker); }, delay);
        }
        private void baseDestroyBlocker(GameObject blocker)
        {
            base.DestroyBlocker(blocker);
        }
    }
}
