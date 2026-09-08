using UnityEngine;

public class Tooltip : MonoBehaviour
{

    private void OnMouseEnter()
    {
        ToolTipManager._instance.SetAndShowToolTip();
    }

    private void OnMouseExit()
    {
        ToolTipManager._instance.HideToolTip();
    }
}