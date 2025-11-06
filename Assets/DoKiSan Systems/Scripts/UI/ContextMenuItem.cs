using System;

public class ContextMenuItem
{
    public string Label;
    public Action Action;
    public Func<string> DynamicLabel;
    public bool CloseOnClick = false; // по умолчанию — НЕ закрывать меню; поведение контролируется тобой

    public string GetLabel()
    {
        return DynamicLabel != null ? DynamicLabel() : Label;
    }
}
