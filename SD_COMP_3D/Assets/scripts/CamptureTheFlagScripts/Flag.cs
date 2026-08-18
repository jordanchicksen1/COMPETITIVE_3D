using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Flag : MonoBehaviour
{
    public enum FlagState
    {
        AtBase,
        Carried,
        Dropped
    }

    public FlagState State { get; private set; } = FlagState.AtBase;
    public GameObject CurrentCarrier { get; private set; }

    [Tooltip("How long after being picked up/stolen this flag is immune to being stolen again. Stops instant ping-pong steals between two nearby players.")]
    public float stealProtectionDuration = 1.5f;
    private float pickedUpAtTime = -999f;

    public bool IsProtectedFromSteal => Time.time - pickedUpAtTime < stealProtectionDuration;

    [Header("Audio")]
    [Tooltip("Played whenever this flag is picked up - from the ground OR via a steal.")]
    public AudioClip pickupSound;

    private Vector3 basePosition;
    private Quaternion baseRotation;
    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        basePosition = transform.position;
        baseRotation = transform.rotation;
    }

    public void PickUp(Transform holdPoint, GameObject carrier)
    {
        State = FlagState.Carried;
        CurrentCarrier = carrier;
        pickedUpAtTime = Time.time;

        CTFAudioManager.Instance.PlayPickupSound();

        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (col != null)
        {
            col.enabled = false;
        }
    }

    public void Drop(Vector3 atPosition)
    {
        State = FlagState.Dropped;
        CurrentCarrier = null;

        transform.SetParent(null);
        transform.position = atPosition + Vector3.up * 0.5f;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void ReturnToBase()
    {
        State = FlagState.AtBase;
        CurrentCarrier = null;

        transform.SetParent(null);
        transform.position = basePosition;
        transform.rotation = baseRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        if (col != null)
        {
            col.enabled = true;
        }
    }
}