using UnityEngine;
using TMPro;

/// <summary>
/// 수류탄 투척 시스템
/// [추가할 곳] Soldier_demo
/// </summary>
public class GrenadeThower : MonoBehaviour
{
    [Header("수류탄 설정")]
    public GameObject grenadePrefab;   // 수류탄 프리팹
    public int        maxGrenades = 5; // 최대 소지 수
    public float      throwForce = 15f; // 던지는 힘

    [Header("던지는 위치")]
    [Tooltip("카메라 앞 던지는 위치. 없으면 카메라 위치 사용")]
    public Transform throwPoint;

    [Header("UI (선택)")]
    public TMP_Text grenadeText;

    // ── 내부 상태 ────────────────────────────────────────
    int _currentGrenades;

    void Start()
    {
        _currentGrenades = maxGrenades;
        UpdateUI();
    }

    void Update()
    {
        // G키로 수류탄 투척
        if (Input.GetKeyDown(KeyCode.G))
            ThrowGrenade();
    }

    // ── 수류탄 투척 ──────────────────────────────────────
    void ThrowGrenade()
    {
        if (_currentGrenades <= 0)
        {
            Debug.Log("수류탄 없음!");
            return;
        }

        if (grenadePrefab == null)
        {
            Debug.LogWarning("GrenadeThower: grenadePrefab 이 연결되지 않았습니다!");
            return;
        }

        // 던지는 위치 결정
        Camera cam      = Camera.main;
        Vector3 spawnPos = throwPoint != null ? throwPoint.position : cam.transform.position;
        Vector3 throwDir = cam.transform.forward;

        // 수류탄 생성
        GameObject grenadeObj = Instantiate(grenadePrefab, spawnPos, cam.transform.rotation);

        // Rigidbody 로 힘 적용
        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);
            // 살짝 회전도 추가 (자연스럽게)
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.VelocityChange);
        }

        _currentGrenades--;
        Debug.Log("Granade!: {_currentGrenades}");

        UpdateUI();
    }

    // ── UI ───────────────────────────────────────────────
    void UpdateUI()
    {
        if (grenadeText != null)
            grenadeText.text = $"Grenades: {_currentGrenades} / {maxGrenades}";
    }
}
