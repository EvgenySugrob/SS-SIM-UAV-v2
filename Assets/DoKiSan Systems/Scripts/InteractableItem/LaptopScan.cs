using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaptopScan : InteractableObject
{
    public override List<ContextMenuItem> GetContextMenuItems()
    {
        return new List<ContextMenuItem>
        {
            new ContextMenuItem{Label = "Переместить", Action = () => PlacementManager.Instance?.StartReposition(this), CloseOnClick=true },
            new ContextMenuItem{Label = "Удалить", Action = () => { Destroy(gameObject); }, CloseOnClick=true }
        };
    }

    
}
