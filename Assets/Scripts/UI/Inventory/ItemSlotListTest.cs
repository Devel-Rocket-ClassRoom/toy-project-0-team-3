using UnityEngine;

public class ItemSlotListTest : MonoBehaviour
{
    public ItemSlotList itemSlotList;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < 5; i++)
            {
                itemSlotList.AddRandomItem();
            }
        }
    }
}
