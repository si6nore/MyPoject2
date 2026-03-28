using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("폭발 설정")]
    public float fuseTime        = 4f;
    public float explosionRadius = 15f;
    public float damage          = 100f;

    [Header("이펙트 설정")]
    public float effectDuration  = 2f;
    public int   sparkCount      = 50;

    float _timer    = 0f;
    bool  _exploded = false;

    void Update()
    {
        if (_exploded) return;
        _timer += Time.deltaTime;
        if (_timer >= fuseTime)
            Explode();
    }

    void Explode()
    {
        _exploded = true;
        CreateExplosionEffect();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            EnemyHealth enemy = col.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            PlayerHealth player = col.GetComponentInParent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(damage);
        }

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        Collider col2 = GetComponent<Collider>();
        if (col2 != null) col2.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Destroy(gameObject, effectDuration + 0.1f);
    }

    void CreateExplosionEffect()
    {
        Vector3 pos = transform.position;

        // ① 불꽃 파티클 (사방으로 튀어나감)
        for (int i = 0; i < sparkCount; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.transform.position   = pos;
            spark.transform.localScale = Vector3.one * Random.Range(0.04f, 0.12f);
            Destroy(spark.GetComponent<Collider>());

            Renderer r   = spark.GetComponent<Renderer>();
            r.material   = new Material(Shader.Find("Sprites/Default"));
            r.material.color = new Color(1f, Random.Range(0.2f, 0.8f), 0f);

            Rigidbody sparkRb  = spark.AddComponent<Rigidbody>();
            sparkRb.AddForce(Random.insideUnitSphere * Random.Range(5f, 14f), ForceMode.VelocityChange);
            sparkRb.useGravity = true;

            float lifetime = Random.Range(0.3f, effectDuration);
            StartCoroutine(FadeOut(r, r.material.color, lifetime));
            Destroy(spark, lifetime);
        }

        // ② 충격파 링
        StartCoroutine(ExpandRing(pos));
    }

    System.Collections.IEnumerator FadeOut(Renderer r, Color startColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (r == null) yield break;
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;
            r.material.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
            yield return null;
        }
    }

    System.Collections.IEnumerator ExpandRing(Vector3 pos)
    {
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ring.transform.position = pos;
        Destroy(ring.GetComponent<Collider>());

        Renderer r   = ring.GetComponent<Renderer>();
        r.material   = new Material(Shader.Find("Sprites/Default"));

        float elapsed  = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            if (ring == null) yield break;
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            ring.transform.localScale = Vector3.one * Mathf.Lerp(1f, 14f, t);
            r.material.color          = new Color(1f, 0.6f, 0.1f, Mathf.Lerp(0.5f, 0f, t));
            yield return null;
        }

        Destroy(ring);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
