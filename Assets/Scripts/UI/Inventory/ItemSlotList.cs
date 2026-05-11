using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotList : MonoBehaviour
{
    public ItemSlot prefab;
    public ScrollRect scrollRect;
    public ItemInfo itemInfo;

    private List<ItemSlot> slotList = new List<ItemSlot>(); // 뷰어
    private List<ItemData> itemDataList = new List<ItemData>(); // 실제 데이터

    private int selectedSlotIndex = -1;
    public int SelectedSlotIndex => selectedSlotIndex;

    private void Awake()
    {
        itemInfo.SetEmpty();
    }

    public void AddItem(string itemId)
    {
        ItemData data = DataTableManager.ItemTable.Get(itemId);
        if (data == null)
            return;

        itemDataList.Add(data);
        UpdateSlots();
    }

    public void AddRandomItem()
    {
        ItemData data = DataTableManager.ItemTable.GetRandom();
        if (data == null)
            return;

        itemDataList.Add(data);
        UpdateSlots();
    }

    public void RemoveItem()
    {
        if (selectedSlotIndex == -1)
            return;

        itemDataList.RemoveAt(selectedSlotIndex);
        selectedSlotIndex = -1;
        UpdateSlots();
    }

    public void Clear()
    {
        itemDataList.Clear();
        selectedSlotIndex = -1;
        UpdateSlots();
    }

    public void Exchange(List<ItemData> chestItemDataList)
    {
        Clear();
        itemDataList = chestItemDataList.ToList<ItemData>();
        UpdateSlots();
    }

    public ItemData GetSelectedItem()
    {
        if (selectedSlotIndex == -1)
            return null;
        return itemDataList[selectedSlotIndex];
    }

    private void UpdateSlots()
    {
        // 슬롯 부족하면 생성
        if (slotList.Count < itemDataList.Count)
        {
            for (int i = slotList.Count; i < itemDataList.Count; i++)
            {
                var slot = Instantiate(prefab, scrollRect.content);
                slot.slotIndex = i;
                slot.gameObject.SetActive(false);

                int capturedIndex = i;
                slot.button.onClick.AddListener(() =>
                {
                    selectedSlotIndex = capturedIndex;
                    itemInfo.SetItemData(itemDataList[capturedIndex]);
                    Debug.Log($"선택된 슬롯: {selectedSlotIndex}");
                });

                slotList.Add(slot);
            }
        }

        // 슬롯 갱신
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < itemDataList.Count)
            {
                slotList[i].gameObject.SetActive(true);
                slotList[i].SetItem(itemDataList[i]);
            }
            else
            {
                slotList[i].gameObject.SetActive(false);
                slotList[i].SetEmpty();
            }
        }

        selectedSlotIndex = -1;
        itemInfo.SetEmpty();
    }
}
