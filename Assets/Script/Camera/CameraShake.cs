using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPos;

    void OnEnable()
    {
        _originalPos = transform.localPosition;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // 무작위 좌표로 카메라 위치 이동
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 흔들림이 끝나면 원래 위치로 복귀
        transform.localPosition = _originalPos;
    }
}