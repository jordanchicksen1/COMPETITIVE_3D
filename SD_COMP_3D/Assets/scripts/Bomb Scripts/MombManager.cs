using System.Collections;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public enum BombType { Sticky, Normal, Bounce, Timer, falling }
    [SerializeField] private BombType bombType;
    [SerializeField]
    private int bounceCount;
    public bool canCheckCollisions;
    [SerializeField]
    private GameObject ExplotionParticles;
    [SerializeField]
    private float fieldOfImpact, explosionForce;
    [SerializeField]
    private float maxDamage = 50f; // Damage at center of explosion

    [SerializeField]
    private GameObject WarningLine;
    private GameObject _spawnedWarning;
    private void Start()
    {
        switch (bombType)
        {
            case BombType.falling:
                SpawnAtRaycastHit();
                break;

        }


    }
    void SpawnAtRaycastHit()
    {

        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 100f;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            _spawnedWarning = Instantiate(WarningLine, hit.point, Quaternion.identity);

            Debug.Log($"Hit {hit.collider.gameObject.name} at {hit.point}");
        }
    }
    public void ActivateBomb()
    {
        switch (bombType)
        {
            case BombType.Timer:
                StartCoroutine(StartTimerBomb());
                break;
        }
    }

    IEnumerator StartTimerBomb()
    {
        yield return new WaitForSeconds(3);
        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canCheckCollisions)
        {
            switch (bombType)
            {
                case BombType.Sticky:
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if (collision.gameObject.CompareTag("Player"))
                    {
                        transform.SetParent(collision.transform);
                    }
                    Destroy(rb);
                    StartCoroutine(StartStickyBomb());
                    break;
                case BombType.Normal:
                    Explode();
                    break;
                case BombType.falling:
                    Explode();
                    break;
                case BombType.Bounce:
                    if (bounceCount == 2)
                    {
                        Explode();
                    }
                    else
                    {
                        bounceCount++;
                    }
                    break;
            }
        }
    }

    IEnumerator StartStickyBomb()
    {
        yield return new WaitForSeconds(5);
        Explode();
    }

    void Explode()
    {
        Debug.Log("Boom");
        GameObject particles = Instantiate(ExplotionParticles, transform.position, Quaternion.identity);
        Collider[] colliders = Physics.OverlapSphere(transform.position, fieldOfImpact);

        foreach (Collider target in colliders)
        {
            Vector3 dir = target.transform.position - transform.position;
            float distance = dir.magnitude;
            float falloff = Mathf.Clamp01(1f - (distance / fieldOfImpact));

            // Handle players (knockback + damage)
            PlayerController3D player = target.GetComponent<PlayerController3D>();
            if (player != null)
            {
                // Knockback
                Vector3 knockback = dir.normalized * explosionForce * falloff;
                knockback.y = 20f;
                player.ApplyKnockback(knockback);

                // Damage
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    float damage = maxDamage * falloff;
                    playerHealth.TakeDamage(damage);
                }

                continue;
            }

            // Handle other rigidbodies (physics objects)
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, fieldOfImpact);
            }
        }

        Destroy(particles, 3);
        Destroy(_spawnedWarning);
        Destroy(gameObject);
    }
}