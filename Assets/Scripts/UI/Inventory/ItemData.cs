using UnityEngine;

public class ItemData
{
    public string Id { get; set; }
    public string ItemName { get; set; }
    public string ItemDesc { get; set; }
    public string Icon { get; set; }
    public int Value { get; set; }

    public Sprite SpriteIcon => Resources.Load<Sprite>($"Icon/{Icon}");
}