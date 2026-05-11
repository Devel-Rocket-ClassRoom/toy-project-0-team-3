using UnityEngine;

public class CreateMap : MonoBehaviour
{
    [Header("Map Tile Generation")]
    public GameObject tilePrefab;
    public int width = 10;
    public int heigth = 10;
    public float tileSize = 10f;

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                Vector3 pos = new Vector3(i * tileSize, 0, j * tileSize);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity);
                tile.transform.parent = this.transform;
                tile.name = $"Tile_{i}_{j}";
            }
        }
    }
}
