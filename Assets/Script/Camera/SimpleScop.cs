using UnityEngine;

public class SimpleScope : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject scopeUI; // 조준선 이미지 UI 오브젝트

    [Header("시야 설정")]
    public float normalFOV = 60f;
    public float zoomFOV = 30f;

    private bool isZoomed = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        mainCamera.fieldOfView = normalFOV;
        if (scopeUI != null) scopeUI.SetActive(false); // 시작할 때 조준선 끄기
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isZoomed = !isZoomed;

            // 카메라 확대/축소
            mainCamera.fieldOfView = isZoomed ? zoomFOV : normalFOV;

            // 조준선 UI 켜기/끄기
            if (scopeUI != null)
            {
                scopeUI.SetActive(isZoomed);
            }
        }
    }
}