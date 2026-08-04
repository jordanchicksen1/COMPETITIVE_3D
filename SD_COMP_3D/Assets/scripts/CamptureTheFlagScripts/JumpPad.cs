using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpPad : MonoBehaviour
{
    public float launchForce = 15f;

    public Vector3 launchDirection = Vector3.up;

    public float relaunchCooldown = 0.5f;

    public bool playJumpAnimation = true;

    private readonly Dictionary<FlagPlayerController, float> lastLaunchTime = new Dictionary<FlagPlayerController, float>();

    void OnTriggerEnter(Collider other)
    {
        TryLaunch(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryLaunch(other);
    }

    private void TryLaunch(Collider other)
    {
        FlagPlayerController player = other.GetComponentInParent<FlagPlayerController>();
        if (player == null)
        {
            return;
        }

        if (lastLaunchTime.TryGetValue(player, out float last) && Time.time - last < relaunchCooldown)
        {
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        Vector3 dir = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector3.up;
        rb.linearVelocity = dir * launchForce;

        lastLaunchTime[player] = Time.time;

        if (playJumpAnimation)
        {
            player.PlayJumpAnimation();
        }
    }
}