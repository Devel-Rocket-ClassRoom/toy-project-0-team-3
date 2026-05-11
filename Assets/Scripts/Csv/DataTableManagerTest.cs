using UnityEngine;

public class DataTableManagerTest : MonoBehaviour
{
    void Start()
    {
        var item = DataTableManager.ItemTable.Get("Item1");
        Debug.Log(item.ItemName);
        Debug.Log(item.ItemDesc);
        Debug.Log(item.Icon);
        Debug.Log(item.Value);
    }
}
