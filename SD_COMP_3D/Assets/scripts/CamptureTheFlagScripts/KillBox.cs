using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillBox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        FlagPlayerController player = other.GetComponentInParent<FlagPlayerController>();
        if (player == null)
        {
            return;
        }

        player.Die(true);
    }
}