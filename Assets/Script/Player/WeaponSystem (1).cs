using UnityEngine;
using TMPro;

public class WeaponSystem : MonoBehaviour
{
    [Header("탄약 설정")]
    public int magazineSize = 30;
    public int totalAmmo    = 120;

    [Header("사격 설정")]
    public float fireRate = 2f;
    public float damage   = 20f;

    [Header("재장전 설정")]
    public float reloadTime = 2.0f;

    [Header("총구 위치 (선택)")]
    public Transform muzzlePoint;

    [Header("총알 이펙트")]
    public float bulletLineDuration = 0.05f;

    [Header("UI (선택) - TextMeshPro")]
    public TMP_Text ammoText;
    public TMP_Text reloadText;

    // ── 내부 상태 ────────────────────────────────────────
    int              _currentAmmo;
    bool             _isReloading  = false;
    float            _nextFireTime = 0f;
    FPSController    _fpsController;  // ★ FPSController 로 변경

    void Start()
    {
        _currentAmmo    = magazineSize;
        _fpsController  = GetComponent<FPSController>();
        UpdateUI();
    }

    void Update()
    {
        if (_isReloading) return;

        if (_currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
            Fire();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
    }

    void Fire()
    {
        _currentAmmo--;
        _nextFireTime = Time.time + (1f / fireRate);

        // ★ FPSController 의 TriggerShoot 호출
        if (_fpsController != null)
            _fpsController.TriggerShoot();

        Camera cam    = Camera.main;
        Ray ray       = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 lineStart = muzzlePoint != null ? muzzlePoint.position : cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            StartCoroutine(DrawBulletLine(lineStart, hit.point));
        }
        else
        {
            StartCoroutine(DrawBulletLine(lineStart, ray.origin + ray.direction * 200f));
        }

        UpdateUI();
    }

    System.Collections.IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("BulletLine");
        LineRenderer lr    = lineObj.AddComponent<LineRenderer>();

        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = Color.yellow;
        lr.endColor      = new Color(1f, 0.5f, 0f);
        lr.startWidth    = 0.02f;
        lr.endWidth      = 0.005f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(bulletLineDuration);
        Destroy(lineObj);
    }

    void TryReload()
    {
        if (totalAmmo <= 0 || _currentAmmo == magazineSize) return;
        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        _isReloading = true;
        if (reloadText != null) reloadText.text = "RELOADING...";

        yield return new WaitForSeconds(reloadTime);

        int fill      = Mathf.Min(magazineSize - _currentAmmo, totalAmmo);
        _currentAmmo += fill;
        totalAmmo    -= fill;

        _isReloading = false;
        if (reloadText != null) reloadText.text = "";

        UpdateUI();
    }

    void UpdateUI()
    {
        if (ammoText != null)
            ammoText.text = $"{_currentAmmo} / {totalAmmo}";
    }

    public int  CurrentAmmo => _currentAmmo;
    public int  TotalAmmo   => totalAmmo;
    public bool IsReloading => _isReloading;
}
