using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSurfaceSpawner : MonoBehaviour
{
    [Header("스폰 대상 설정")]
    [Tooltip("적이 스폰될 수 있는 플랫폼 오브젝트")]
    public List<GameObject> spawnPlatforms;
    public GameObject enemyPrefab;

    [Header("웨이브 설정")]
    public int maxSpawnCount = 10;
    public float spawnDelay = 2f;

    [Header("상태 확인")]
    public bool isWaveActive = false;
    public int currentSpawnedCount = 0;
    public bool isWaitingForNextSpawn = false;

    private GameObject _currentEnemy;
    private EnemyHealth _currentEnemyHealth;

    void Update()
    {
        // J 키로 웨이브 토글
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleWave();
        }

        if (isWaveActive && !isWaitingForNextSpawn)
        {
            CheckAndSpawnNext();
        }
    }

    void ToggleWave()
    {
        isWaveActive = !isWaveActive;
        if (isWaveActive)
        {
            Debug.Log("<color=cyan>오브젝트 기반 웨이브 시작! (J)</color>");
            currentSpawnedCount = 0;
            isWaitingForNextSpawn = false;
        }
        else
        {
            Debug.Log("<color=orange>오브젝트 기반 웨이브 중단!</color>");
            StopAllCoroutines();
        }
    }

    void CheckAndSpawnNext()
    {
        if (currentSpawnedCount < maxSpawnCount)
        {
            if (_currentEnemy == null || (_currentEnemyHealth != null && _currentEnemyHealth.IsDead))
            {
                StartCoroutine(WaitAndSpawn());
            }
        }
        else if (currentSpawnedCount >= maxSpawnCount && (_currentEnemy == null || _currentEnemyHealth.IsDead))
        {
            Debug.Log("모든 오브젝트 구역 적 처치 완료!");
            isWaveActive = false;
        }
    }

    IEnumerator WaitAndSpawn()
    {
        isWaitingForNextSpawn = true;

        if (currentSpawnedCount > 0)
            Debug.Log($"{spawnDelay}초 후 다음 플랫폼에서 적이 나타납니다...");

        yield return new WaitForSeconds(spawnDelay);

        if (isWaveActive)
        {
            SpawnEnemyOnObject();
        }

        isWaitingForNextSpawn = false;
    }

    void SpawnEnemyOnObject()
    {
        if (spawnPlatforms == null || spawnPlatforms.Count == 0)
        {
            Debug.LogWarning("스폰할 플랫폼 오브젝트가 리스트에 없습니다!");
            isWaveActive = false;
            return;
        }

        currentSpawnedCount++;

        // 1. 리스트 중 랜덤하게 하나의 오브젝트 선택
        int randomIndex = Random.Range(0, spawnPlatforms.Count);
        GameObject targetPlatform = spawnPlatforms[randomIndex];

        // 2. 선택된 오브젝트의 Collider를 기반으로 상단 위치 계산
        Collider col = targetPlatform.GetComponent<Collider>();
        Vector3 spawnPos;

        if (col != null)
        {
            Bounds bounds = col.bounds;
            // X, Z는 플랫폼의 가로세로 범위 내 랜덤, Y는 플랫폼의 가장 윗부분
            float rx = Random.Range(bounds.min.x, bounds.max.x);
            float rz = Random.Range(bounds.min.z, bounds.max.z);
            float ry = bounds.max.y; // 오브젝트의 맨 윗면

            spawnPos = new Vector3(rx, ry, rz);
        }
        else
        {
            // Collider가 없으면 그냥 오브젝트의 중심 위치 사용
            spawnPos = targetPlatform.transform.position;
        }

        _currentEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        _currentEnemyHealth = _currentEnemy.GetComponent<EnemyHealth>();

        Debug.Log($"{targetPlatform.name} 위에서 적 스폰! ({currentSpawnedCount}/{maxSpawnCount})");
    }
}