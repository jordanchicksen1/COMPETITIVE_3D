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

    [Tooltip("Which team this flag belongs to. Used by FlagCaptureZone to tell friendly vs enemy flag.")]
    public int TeamId = 0;

    public FlagState State { get; private set; } = FlagState.AtBase;
    public GameObject CurrentCarrier { get; private set; }

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

        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Disabling the collider stops it from immediately re-triggering
        // pickup logic while it's riding around on the player.
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

    // Sends the flag back to its home base, e.g. after a successful capture
    // or after a "return the flag" timeout / manual touch on a dropped flag.
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