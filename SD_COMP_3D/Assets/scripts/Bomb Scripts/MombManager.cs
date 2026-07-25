using System.Collections;
using UnityEngine;
public class BombManager : MonoBehaviour
{
    public enum BombType { Sticky, Normal, Bounce, Timer }
    [SerializeField] private BombType bombType;
    [SerializeField]
    private int bounceCount;
    public bool canCheckCollisions;
    [SerializeField]
    private GameObject ExplotionParticles;
    [SerializeField]
    private float fieldOfImpact, explosionForce;
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
            // Players move via MovePosition, so a regular physics force does
            // nothing to them - push them through their own knockback channel instead.
            PlayerController3D player = target.GetComponent<PlayerController3D>();
            if (player != null)
            {
                Vector3 dir = target.transform.position - transform.position;
                float distance = dir.magnitude;
                float falloff = Mathf.Clamp01(1f - (distance / fieldOfImpact));
                Vector3 knockback = dir.normalized * explosionForce * falloff;
                knockback.y = 20; // keep the push horizontal; add a vertical pop here if you want a launch effect
                player.ApplyKnockback(knockback);
                continue;
            }

            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, fieldOfImpact);
            }
        }
        Destroy(particles, 3);
        Destroy(gameObject);
    }
}