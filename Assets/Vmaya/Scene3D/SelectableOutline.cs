
namespace Vmaya.Scene3D
{
    public class SelectableOutline : BaseSelectableOutline
    {
        public virtual bool isAllowed()
        {
            return enabled;
        }
    }
}