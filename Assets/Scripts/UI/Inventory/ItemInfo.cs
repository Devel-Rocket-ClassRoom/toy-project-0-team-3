using TMPro;
using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    public GameObject itemName;
    public GameObject itemExplain;
    public GameObject removeButton;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textExplain;

    public void SetEmpty()
    {
        itemName.SetActive(false);
        itemExplain.SetActive(false);
        removeButton.SetActive(false);
        textName.text = string.Empty;
        textExplain.text = string.Empty;
    }

    public void SetItemData(ItemData itemData)
    {
        itemName.SetActive(true);
        itemExplain.SetActive(true);
        removeButton.SetActive(true);
        textName.text = itemData.ItemName;
        textExplain.text = itemData.ItemDesc;
    }
}
