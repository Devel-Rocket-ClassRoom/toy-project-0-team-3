using UnityEngine;
using System.Collections;

public class RandomMapGenerator : MonoBehaviour
{
    // [Header("Monster Spawner")]
    // public MonsterRandomSpawner monsterSpawner;

    [Header("Map Tile Prefabs")]
    public GameObject[] tilePrefabs; // 맵 프리팹 배열 
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Grid Settings")]
    public float tileSize = 20f; // 타일 한 칸 크기
    // 현재 맵
    private GameObject _currentMap;
    // 현재 플레이어 캐릭터
    public GameObject _currentPlayer;
    // 플레이서 생성 포지션 
    private Vector3 _playerPosition = Vector3.zero;
    private Vector3 _playerPositionLifter = new Vector3 (0f, 1f, 0f);
    
    private void Start()
    {
        SpawnRandomTile();
        // _currentPlayer = Instantiate(, Vector3.zero, Quaternion.identity);
        // _currentPlayer.transform.SetParent(transform);
        DeActivateTileFloors();
    }

    private void Update()
    {
        // 스페이스 시 기존 맵을 제거 하고 새로운 랜덤 맵타일 스폰
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     SpawnRandomTile();
        // }
    }

    // 랜덤 맵타일 스폰 메서드
    private void SpawnRandomTile()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogWarning("RandomMapGenerator : tilePrefabs가 비어있습니다.");
            return;
        }

        // if (_currentMap != null)
        // {
        //     Destroy(_currentMap);
        // }

        _currentMap = new GameObject("MapGrid");
        _currentMap.transform.SetParent(transform);
        _currentMap.transform.localPosition = Vector3.zero;

        // 그리그 중앙 정렬 오프셋 (3칸 기준: -tileSize, 0, +tileSize)
        float offset = tileSize;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                // int index = Random.Range(0, tilePrefabs.Length);
                // Vector3 pos = new Vector3((i - 1) * tileSize, 0, (j - 1) * tileSize);
                // GameObject tile = Instantiate (tilePrefabs[index], pos , Quaternion.identity);
                // tile.transform.SetParent(_currentMap.transform);


                int index = Random.Range(0, tilePrefabs.Length);
                Vector3 targetPos = new Vector3 ((i - 1) * tileSize, 0, (j - 1) * tileSize);
                GameObject tile = Instantiate (tilePrefabs[index], targetPos, Quaternion.identity);
                tile.transform.SetParent(_currentMap.transform);

                // 납작한 바닥 렌더러들만 모아서 전체 바닥 중심을 targetPos에 정렬
                Renderer[] allRenderers = tile.GetComponentsInChildren<Renderer>();
                Bounds floorBounds = new();
                bool boundsInitialized = false;
                foreach (Renderer r in allRenderers)
                {
                    if (r.bounds.size.y < 0.5f)
                    {
                        if (!boundsInitialized) { floorBounds = r.bounds; boundsInitialized = true; }
                        else floorBounds.Encapsulate(r.bounds);
                    }
                }
                if (boundsInitialized)
                {
                    tile.transform.position += new Vector3(
                        targetPos.x - floorBounds.center.x,
                        0f,
                        targetPos.z - floorBounds.center.z
                    );
                }

                // _currentPlayer.transform.position = _playerPosition;

                // StartCoroutine(BakeAndSpawnRoutine());

            }
        }
        Debug.Log ("3x3 맵 생성 완료");

        // _currentMap = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity);
        // _currentMap.transform.SetParent(transform);
        // Debug.Log ($"맵 생성 : {tilePrefabs[index].name} (index{index})");

        _currentPlayer.transform.position = _playerPosition + _playerPositionLifter;
        Debug.Log (_currentPlayer.transform.position);
        //Instantiate (playerPrefab, Vector3.zero + _playerPositionLifter, Quaternion.identity);
    }

    private void DeActivateTileFloors()
    {
        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("TileFloor");
        foreach (GameObject floorTile in floorTiles)
        {
            Destroy(floorTile);
            // floorTile.SetActive(false);
        }
    }

    // private IEnumerator BakeAndSpawnRoutine()
    // {
    //     yield return null;

    //     if (monsterSpawner != null)
    //     {
    //         monsterSpawner.BakeAndSpawn();
    //     }
    // }
}
