using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemTable : DataTable
{
    public readonly Dictionary<string, ItemData> table =
        new Dictionary<string, ItemData>();

    private List<string> keyList;

    public override void Load(string filename)
    {
        table.Clear();

        string path = string.Format(FormatPath, filename);
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        List<ItemData> list = LoadCSV<ItemData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Id))
            {
                table.Add(item.Id, item);
            }
            else
            {
                Debug.LogError("아이템 아이디 중복");
            }
        }

        keyList = table.Keys.ToList();
    }

    public ItemData Get(string id)
    {
        if (!table.ContainsKey(id))
        {
            Debug.LogError("아이템 아이디 없음");
            return null;
        }

        return table[id];
    }

    public ItemData GetRandom()
    {
        return Get(keyList[Random.Range(0, keyList.Count)]);
    }
}
