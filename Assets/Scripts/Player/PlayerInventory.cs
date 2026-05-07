using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private PlayerInput _playerInput;

    public ItemSlotList chestItemSlotList;
    public ItemSlotList inventoryItemSlotList;

    private GameObject _chest;
    

    public GameObject _inventory;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inventory.SetActive(false);
    }

    private void Update()
    {
        if (_playerInput.TabKey)
        {
            _inventory.SetActive(!_inventory.activeSelf);
        } 
    }

    public void OnClickObtain()
    {
        inventoryItemSlotList.AddItem(chestItemSlotList.GetSelectedItem().Id);
        chestItemSlotList.RemoveItem();
        _chest.GetComponent<Chest>().itemDataList.RemoveAt(chestItemSlotList.SelectedSlotIndex);
    }

    public void SetChest(GameObject chest)
    {  
        _chest = chest;
    }
}
