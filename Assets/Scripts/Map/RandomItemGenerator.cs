using System.Collections;
using UnityEngine;

public class RandomItemGenerator : MonoBehaviour
{
    [Header("Spawnable Items")]
    public GameObject[] spawnableItems;

    [Header("Spawn Settings")]
    public int itemCount = 10;
    public float spawnHeight = 1f;
    public float edgePadding = 0.5f;

    private GameObject[] spawnedMapTiles;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(SpawnItemsRoutine());
        }
    }

    private void SpawnItems()
    {
        int randomIndex = Random.Range(0, spawnableItems.Length);
        Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
    }

    private IEnumerator SpawnItemsRoutine()
    {
        yield return null;

        // MapTile 태그로 생성된 맵타일 찾기
        GameObject[] mapTiles = GameObject.FindGameObjectsWithTag("MapTile");

        if (mapTiles.Length == 0)
        {
            Debug.LogError("맵 타일이 존재하지 않습니다!");
            yield break;
        }
        Debug.Log($"현재 맵 타일: {mapTiles.Length}개");

        for (int i = 0; i < itemCount; i++)
        {
            // 랜덤한 맵타일 한칸 선택
            GameObject randomTile = mapTiles[Random.Range(0, mapTiles.Length)];

            // 해당 타일의 크기 계싼
            Bounds tileBounds = GetTileBounds(randomTile);

            // Bounds 내부의 랜덤 좌표 계산
            float randomX = Random.Range(
                tileBounds.min.x + edgePadding,
                tileBounds.max.x - edgePadding
            );

            float randomZ = Random.Range(
                tileBounds.min.z + edgePadding,
                tileBounds.max.z - edgePadding
            );

            float spawnY = tileBounds.max.y + spawnHeight;

            Vector3 spawnPos = new Vector3(randomX, spawnY, randomZ);

            // 인스턴스 생성
            GameObject randomItem = spawnableItems[Random.Range(0, spawnableItems.Length)];
            Instantiate(randomItem, spawnPos, Quaternion.identity, transform);
        }
    }

    private Bounds GetTileBounds(GameObject tile)
    {
        // Collider에서 bounds 계산
        Collider col = tile.GetComponentInChildren<Collider>();
        if (col != null)
            return col.bounds;

        // collider로 계산 불가하면 Renderer로 계산
        Renderer rend = tile.GetComponentInChildren<Renderer>();
        if (rend != null)
            return rend.bounds;

        // 둘다 해당안되면 기본 transform 위치 기준으로 임의의 bounds 계산
        return new Bounds(tile.transform.position, Vector3.one);
    }
}
