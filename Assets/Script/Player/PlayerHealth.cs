using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력")]
    public float maxHealth = 100f;
    private float _currentHealth;
    private bool _isDead = false;

    [Header("피격 효과 (UI)")]
    public Image hitFlashImage;
    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("게임 오버 설정")]
    public GameObject gameOverPanel; // 1단계에서 만든 Panel 연결

    [Header("카메라 흔들림")]
    public CameraShake cameraShake;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.2f;

    void Start()
    {
        _currentHealth = maxHealth;

        if (hitFlashImage != null)
        {
            Color c = hitFlashImage.color;
            c.a = 0f;
            hitFlashImage.color = c;
            hitFlashImage.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // 시작 시 꺼두기
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"플레이어 체력: {_currentHealth}");

        // 피격 효과
        if (hitFlashImage != null)
        {
            StopCoroutine("FlashRedScreen");
            StartCoroutine(FlashRedScreen());
        }

        if (cameraShake != null)
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

        if (_currentHealth <= 0)
        {
            _isDead = true;
            Die();
        }
    }

    IEnumerator FlashRedScreen()
    {
        // 1. 이미지가 연결되어 있다면 오브젝트를 활성화
        if (hitFlashImage != null)
        {
            hitFlashImage.gameObject.SetActive(true);
            hitFlashImage.color = flashColor;
        }

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            if (hitFlashImage != null)
            {
                // 서서히 투명해짐
                hitFlashImage.color = Color.Lerp(flashColor, new Color(1f, 0f, 0f, 0f), elapsed / flashDuration);
            }
            yield return null;
        }

        // 2. 효과가 끝나면 다시 오브젝트를 비활성화해서 화면을 깔끔하게 유지
        if (hitFlashImage != null)
        {
            hitFlashImage.gameObject.SetActive(false);
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망!");

        // 1. 게임 오버 UI 띄우기
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // 2. 커서 해제 (메뉴 조작 등을 위해)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. 게임 종료 실행
        Invoke("QuitGame", 3f); // 3초 뒤에 종료 (너무 갑자기 꺼지면 놀라니까요!)
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 멈춤
#else
            Application.Quit(); // 빌드된 게임 종료
#endif
    }

    public float CurrentHealth => _currentHealth;
    public bool IsDead => _isDead;
}