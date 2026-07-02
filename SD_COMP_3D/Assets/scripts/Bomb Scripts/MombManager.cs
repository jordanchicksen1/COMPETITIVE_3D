using System.Collections;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public enum BombType { Sticky, Normal, Bounce, Timer }

    [SerializeField] private BombType bombType;
    [SerializeField]
    private int bounceCount;
    public bool canCheckCollisions;

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
        Destroy(gameObject);
    }
}