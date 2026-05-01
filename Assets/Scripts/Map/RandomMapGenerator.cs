using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [Header("Map Tile Prefabs")]
    public GameObject[] tilePrefabs; // 맵 프리팹 배열 
    [Header("Player Prefab")]
    public GameObject playerPrefab;
    // 현재 맵
    private GameObject _currentMap;
    // 현재 플레이어 캐릭터
    private GameObject _currentPlayer;


    private void Start()
    {
        SpawnRandomTile();
        _currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        _currentPlayer.transform.SetParent(transform);
    }

    private void Update()
    {
        // 스페이스 시 기존 맵을 제거 하고 새로운 랜덤 맵타일 스폰
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnRandomTile();
        }
    }

    // 랜덤 맵타일 스폰 메서드
    private void SpawnRandomTile()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogWarning("RandomMapGenerator : tilePrefabs가 비어있습니다.");
            return;
        }

        if (_currentMap != null)
        {
            Destroy(_currentMap);
        }
        if (_currentPlayer != null)
        {
            Destroy(_currentPlayer);
        }

        int index = Random.Range(0, tilePrefabs.Length);
        _currentMap = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity);
        _currentMap.transform.SetParent(transform);
        Debug.Log ($"맵 생성 : {tilePrefabs[index].name} (index{index})");

        if (playerPrefab != null)
        {
            _currentPlayer = Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
        }

        Debug.Log ($"플레이어 생성 : {playerPrefab.name}");
    }
}
