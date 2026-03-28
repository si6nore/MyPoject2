using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 AI - 순찰 → 플레이어 발견 → 추격 → 사격
/// [추가할 곳] Enemy 프리팹
/// [필요 컴포넌트] NavMeshAgent, EnemyHealth
/// Animator 파라미터: IsRunning (Bool), Shoot (Trigger)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Attack, Dead }
    State _state = State.Patrol;

    [Header("탐지")]
    public float sightRange  = 20f;
    public float sightAngle  = 120f;
    public float attackRange = 12f;

    [Header("순찰")]
    public float patrolRadius = 15f;
    public float patrolWait   = 2f;

    [Header("사격")]
    public float fireRate     = 1.5f;
    public float bulletDamage = 10f;
    public float bulletRange  = 50f;

    [Header("총알 이펙트")]
    public float bulletLineDuration = 0.05f;

    [Header("이동 속도")]
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 4.5f;

    // ── 내부 상태 ────────────────────────────────────────
    NavMeshAgent _agent;
    EnemyHealth  _health;
    Animator     _animator;
    Transform    _player;
    float        _nextFireTime = 0f;
    float        _patrolTimer  = 0f;
    Vector3      _patrolOrigin;

    void Start()
    {
        _agent        = GetComponent<NavMeshAgent>();
        _health       = GetComponent<EnemyHealth>();
        _animator     = GetComponentInChildren<Animator>();
        _patrolOrigin = transform.position;

        if (_animator == null)
            Debug.LogWarning($"{gameObject.name}: Animator 없음!");

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) _player = p.transform;
        else Debug.LogWarning("EnemyAI: Player 태그 오브젝트를 찾지 못했습니다!");

        SetNextPatrolPoint();
    }

    void Update()
    {
        if (_health.IsDead)
        {
            if (_state != State.Dead)
            {
                _state = State.Dead;
                SetStopped(true);
                SetAnim(false); // 사망 시 이동 애니 끄기
            }
            return;
        }

        switch (_state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }

        UpdateAnimation();
    }

    // ── 순찰 ─────────────────────────────────────────────
    void UpdatePatrol()
    {
        _agent.speed = patrolSpeed;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= patrolWait)
            {
                _patrolTimer = 0f;
                SetNextPatrolPoint();
            }
        }

        if (CanSeePlayer())
        {
            Debug.Log($"{gameObject.name} 플레이어 발견!");
            _state = State.Chase;
        }
    }

    // ── 추격 ─────────────────────────────────────────────
    void UpdateChase()
    {
        _agent.speed = chaseSpeed;

        if (_player == null) { _state = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackRange)
        {
            SetStopped(true);
            _state = State.Attack;
            return;
        }

        if (dist > sightRange * 1.5f)
        {
            _state = State.Patrol;
            SetNextPatrolPoint();
            return;
        }

        SetStopped(false);
        if (_agent.isOnNavMesh)
            _agent.SetDestination(_player.position);
    }

    // ── 사격 ─────────────────────────────────────────────
    void UpdateAttack()
    {
        if (_player == null) { _state = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);

        if (dist > attackRange * 1.2f)
        {
            SetStopped(false);
            _state = State.Chase;
            return;
        }

        if (Time.time >= _nextFireTime)
            Shoot();
    }

    // ── 애니메이션 업데이트 ──────────────────────────────
    void UpdateAnimation()
    {
        if (_animator == null) return;

        // NavMeshAgent 실제 이동 속도로 판단
        Vector3 flatVel  = new Vector3(_agent.velocity.x, 0f, _agent.velocity.z);
        bool    isMoving = flatVel.magnitude > 0.1f;

        _animator.SetBool("IsRunning", isMoving);
    }

    // ── 발사 ─────────────────────────────────────────────
    void Shoot()
    {
        _nextFireTime = Time.time + (1f / fireRate);

        // ★ 사격 애니메이션
        if (_animator != null)
            _animator.SetTrigger("Shoot");

        if (_player == null) return;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = _player.position   + Vector3.up * 1f;
        Vector3 dir    = (target - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, bulletRange))
        {
            StartCoroutine(DrawBulletLine(origin, hit.point));

            PlayerHealth player = hit.collider.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(bulletDamage);
                Debug.Log($"적 → 플레이어 데미지 {bulletDamage}");
            }
        }
        else
        {
            StartCoroutine(DrawBulletLine(origin, origin + dir * bulletRange));
        }
    }

    // ── 총알 선 이펙트 ───────────────────────────────────
    System.Collections.IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("EnemyBulletLine");
        LineRenderer lr    = lineObj.AddComponent<LineRenderer>();

        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = new Color(1f, 0.3f, 0.3f);
        lr.endColor      = new Color(1f, 0.6f, 0f);
        lr.startWidth    = 0.02f;
        lr.endWidth      = 0.005f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(bulletLineDuration);
        Destroy(lineObj);
    }

    // ── 플레이어 발견 체크 ───────────────────────────────
    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 origin    = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = _player.position   + Vector3.up * 1f;
        Vector3 dir       = (targetPos - origin).normalized;
        float   dist      = Vector3.Distance(origin, targetPos);

        if (dist > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > sightAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            if (hit.collider.GetComponentInParent<PlayerHealth>() != null ||
                hit.collider.CompareTag("Player") ||
                hit.transform.root == _player.root)
                return true;

            return false;
        }

        return true;
    }

    // ── 다음 순찰 지점 ───────────────────────────────────
    void SetNextPatrolPoint()
    {
        Vector2 rand   = Random.insideUnitCircle * patrolRadius;
        Vector3 target = _patrolOrigin + new Vector3(rand.x, 0f, rand.y);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            SetStopped(false);
            if (_agent.isOnNavMesh)
                _agent.SetDestination(hit.position);
        }
    }

    // ── 애니메이션 헬퍼 ──────────────────────────────────
    void SetAnim(bool isRunning)
    {
        if (_animator != null)
            _animator.SetBool("IsRunning", isRunning);
    }

    // ── 안전한 isStopped 설정 ────────────────────────────
    void SetStopped(bool stopped)
    {
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = stopped;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
