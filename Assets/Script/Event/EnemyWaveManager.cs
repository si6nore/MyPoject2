using UnityEngine;
using System.Collections; // Coroutine 사용을 위해 필요

public class EnemyWaveManager : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public int maxSpawnCount = 10;
    public float spawnDelay = 2f;    // 2초의 대기 시간

    [Header("상태 확인")]
    public bool isWaveActive = false;
    public int currentSpawnedCount = 0;
    public bool isWaitingForNextSpawn = false; // 대기 중인지 체크

    private BoxCollider _spawnArea;
    private GameObject _currentEnemy;
    private EnemyHealth _currentEnemyHealth;

    void Awake()
    {
        _spawnArea = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
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
            Debug.Log("<color=green>웨이브 시작!</color>");
            currentSpawnedCount = 0;
            isWaitingForNextSpawn = false;
        }
        else
        {
            Debug.Log("<color=red>웨이브 중단!</color>");
            StopAllCoroutines(); // 중단 시 대기 중인 스폰도 취소
        }
    }

    void CheckAndSpawnNext()
    {
        if (currentSpawnedCount < maxSpawnCount)
        {
            // 현재 적이 없거나 죽었는지 확인
            if (_currentEnemy == null || (_currentEnemyHealth != null && _currentEnemyHealth.IsDead))
            {
                // 바로 스폰하지 않고 코루틴 시작
                StartCoroutine(WaitAndSpawn());
            }
        }
        else if (currentSpawnedCount >= maxSpawnCount && _currentEnemyHealth != null && _currentEnemyHealth.IsDead)
        {
            Debug.Log("모든 적 처치 완료!");
            isWaveActive = false;
        }
    }

    // 2초 대기 후 스폰하는 핵심 로직
    IEnumerator WaitAndSpawn()
    {
        isWaitingForNextSpawn = true; // 중복 실행 방지

        if (currentSpawnedCount > 0) // 첫 스폰이 아닐 때만 메시지 출력
            Debug.Log($"{spawnDelay}초 후 다음 적이 나타납니다...");

        yield return new WaitForSeconds(spawnDelay);

        if (isWaveActive) // 대기 중에 H를 눌러 껐을 수도 있으므로 다시 확인
        {
            SpawnEnemy();
        }

        isWaitingForNextSpawn = false;
    }

    void SpawnEnemy()
    {
        currentSpawnedCount++;

        Bounds bounds = _spawnArea.bounds;
        float rx = Random.Range(bounds.min.x, bounds.max.x);
        float ry = bounds.min.y;
        float rz = Random.Range(bounds.min.z, bounds.max.z);
        Vector3 spawnPos = new Vector3(rx, ry, rz);

        _currentEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        _currentEnemyHealth = _currentEnemy.GetComponent<EnemyHealth>();

        Debug.Log($"적 스폰 ({currentSpawnedCount}/{maxSpawnCount})");
    }
}