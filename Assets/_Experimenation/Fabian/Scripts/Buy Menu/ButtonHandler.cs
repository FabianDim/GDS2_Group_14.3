using UnityEngine;

public abstract class ButtonHandler : MonoBehaviour
{
    // This method must be public to show up in the onClick() inspector
    public abstract void OnButtonClick();

    // You can also pass parameters
    public void OnItemClick(string itemName)
    {
        Debug.Log("Clicked on item: " + itemName);
    }
}