using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 체력 시스템
/// 캐릭터 오브젝트에 추가
/// </summary>
class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;

    [Header("사망 시 회전 (쓰러지는 방향)")]
    public Vector3 deathRotation = new Vector3(90f, 0f, 0f); // 앞으로 쓰러짐
    public float deathRotateSpeed = 5f;                     // 쓰러지는 속도

    // ── 내부 상태 ────────────────────────────────────────
    float _currentHealth;
    bool _isDead = false;

    void Start()
    {
        _currentHealth = maxHealth;
    }

    void Update()
    {
        // 사망 시 서서히 회전
        if (_isDead)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(deathRotation),
                Time.deltaTime * deathRotateSpeed
            );
        }
    }

    // ── 데미지 받기 ──────────────────────────────────────
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} 체력: {_currentHealth} / {maxHealth}");

        if (_currentHealth <= 0)
            Die();
    }

    // ── 사망 처리 ────────────────────────────────────────
    void Die()
    {
        _isDead = true;
        _currentHealth = 0;

        Debug.Log($"{gameObject.name} 사망");

        // 콜라이더 끄기 (죽은 후 총알 안 맞게)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // NavMeshAgent 끄기 (죽은 후 안 움직이게)
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // ★★★ 1초 뒤에 맵에서 사라지게 설정 ★★★
        // 기존 주석(//)을 해제하고 시간을 1f로 수정했습니다.
        Destroy(gameObject, 1f);
    }

    // ── 외부에서 상태 확인용 ─────────────────────────────
    public bool IsDead => _isDead;
    public float CurrentHealth => _currentHealth;
}