using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MonsterRandomSpawner : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject[] monsterPrefabs;
    public int spawnCount = 10;

    [Header("Spawn Rulse")]
    public float minDistanceBetweenSpawns = 3f;
    public float minDistanceFromPlayer = 8f;
    public float sampleRadius = 1f;
    public float flatNormalThreshold = 0.95f;
    public int maxSampleAttempts = 100;

    [Header("References")]
    public NavMeshSurface navMeshSurface;
    public Transform player;

    private List<Vector3> _spawnedPositions = new();
    private List<GameObject> _spawnedMonsters = new();

    private void Start()
    {
        ClearMonsters();
        SpawnMonsters();
    }

    // public void BakeAndSpawn()
    // {
    //     ClearMonsters();
    //     navMeshSurface.BuildNavMesh();
    //     SpawnMonsters();
    // }

    private void SpawnMonsters()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            Debug.Log("MonsterSpawner : monsterPrefabs가 비어있습니다.");
            return;
        }

        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        List<int> flatTriangleIndices = new();
        List<float> cumulativeArea = new();
        float totalArea = 0f;

        for (int i = 0; i < tri.indices.Length; i += 3)
        {
            Vector3 a = tri.vertices[tri.indices[i]];
            Vector3 b = tri.vertices[tri.indices[i + 1]];
            Vector3 c = tri.vertices[tri.indices[i + 2]];

            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

            if (Vector3.Dot(normal, Vector3.up) < flatNormalThreshold)
            {
                continue;
            }

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            flatTriangleIndices.Add(i);
            totalArea += area;
            cumulativeArea.Add(totalArea);
        }

        if (flatTriangleIndices.Count == 0)
        {
            Debug.Log("MonsterSpawner : NavMesh의 Flat Triangle가 없습니다.");
            return;
        }

        int spawned = 0;
        int attempts = 0;
        while (spawned < spawnCount && attempts < maxSampleAttempts)
        {
            attempts++;

            float pick = Random.Range(0f, totalArea);
            int triListIdx = cumulativeArea.BinarySearch(pick);

            if (triListIdx < 0)
            {
                triListIdx = ~triListIdx;
            }
            triListIdx = Mathf.Clamp(triListIdx, 0, flatTriangleIndices.Count - 1);

            int idx = flatTriangleIndices[triListIdx];
            Vector3 a = tri.vertices[tri.indices[idx]];
            Vector3 b = tri.vertices[tri.indices[idx + 1]];
            Vector3 c = tri.vertices[tri.indices[idx + 2]];

            float r1 = Random.value;
            float r2 = Random.value;

            if (r1 + r2 > 1f)
            {
                r1 = 1f - r1;
                r2 = 1f - r2;
            }
            Vector3 candidate = a + r1 * (b - a) + r2 * (c - a);

            if (
                !NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    sampleRadius,
                    NavMesh.AllAreas
                )
            )
            {
                continue;
            }

            if (
                player != null
                && Vector3.Distance(hit.position, player.position) < minDistanceFromPlayer
            )
            {
                continue;
            }

            bool tooClose = false;
            foreach (var pos in _spawnedPositions)
            {
                if (Vector3.Distance(pos, hit.position) < minDistanceBetweenSpawns)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
            {
                continue;
            }

            GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
            GameObject monster = Instantiate(
                prefab,
                hit.position,
                Quaternion.Euler(0, Random.Range(0f, 360f), 0)
            );
            monster.transform.SetParent(transform);

            _spawnedPositions.Add(hit.position);
            ;
            _spawnedMonsters.Add(monster);
            spawned++;
        }

        Debug.Log($"몬스터 {spawned} / {spawnCount} 스폰 완료 (시도: {attempts}).");
    }

    public void ClearMonsters()
    {
        foreach (var m in _spawnedMonsters)
        {
            if (m != null)
            {
                Destroy(m);
            }
        }
        _spawnedMonsters.Clear();
        _spawnedPositions.Clear();
    }
}
