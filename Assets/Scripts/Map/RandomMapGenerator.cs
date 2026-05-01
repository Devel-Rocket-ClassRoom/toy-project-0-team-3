using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [Header("Map Tile Prefabs")]
    public GameObject[] tilePrefabs; // 맵 프리팹 배열 
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Grid Settings")]
    public float tileSize = 20f; // 타일 한 칸 크기
    // 현재 맵
    private GameObject _currentMap;
    // 현재 플레이어 캐릭터
    private GameObject _currentPlayer;
    // 플레이서 생성 포지션 
    private Vector3 _playerPosition;
    
    // 임시로 플레이어 생성 포지션 y좌표 1로 올려줌 (절반이 밑으로 들어가서)
    private Vector3 _playerPositionLifter = new Vector3(0f, 1f, 0f);

    private void Start()
    {
        SpawnRandomTile();
        // _currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        // _currentPlayer.transform.SetParent(transform);
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
            Debug.Log ($"기존 플레이어 제거 : {_currentPlayer.name}");  
            Destroy(_currentPlayer); 
        }

        // 3x3 맵 생성해서 담을 빈 부모 생성
        _currentMap = new GameObject("MapGrid");
        _currentMap.transform.SetParent(transform);
        _currentMap.transform.localPosition = Vector3.zero;

        // 그리그 중앙 정렬 오프셋 (3칸 기준: -tileSize, 0, +tileSize)
        float offset = tileSize;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int index = Random.Range(0, tilePrefabs.Length);
                Vector3 pos = new Vector3((i - 1) * tileSize, 0, (j - 1) * tileSize);
                GameObject tile = Instantiate (tilePrefabs[index], pos , Quaternion.identity);
                tile.transform.SetParent(_currentMap.transform);
            }
        }
        Debug.Log ("3x3 맵 생성 완료");

        // _currentMap = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity);
        // _currentMap.transform.SetParent(transform);
        // Debug.Log ($"맵 생성 : {tilePrefabs[index].name} (index{index})");

        if (playerPrefab != null)
        {
            _currentPlayer = Instantiate (playerPrefab, Vector3.zero + _playerPositionLifter, Quaternion.identity);
        }

        Debug.Log ($"플레이어 생성 : {playerPrefab.name}");
    }
}
