using UnityEngine;

public enum BuyCategory
{
}

[CreateAssetMenu(menuName = "Buy Menu/Item")]
public class BuyMenuItem : ScriptableObject
{
    public string itemName;
    public int price;
    public Sprite image;
    public KeyCode keybind;
    public string inputLabel;
    public BuyCategory category;
    public GameObject itemPrefab;
}