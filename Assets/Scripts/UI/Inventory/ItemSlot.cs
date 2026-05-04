using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public int slotIndex = -1;
    public Image icon;
    public Button button;

    public void SetItem(ItemData data)
    {
        icon.sprite = data.SpriteIcon;
        gameObject.SetActive(true);
    }

    public void SetEmpty()
    {
        icon.sprite = null;
    }
}
