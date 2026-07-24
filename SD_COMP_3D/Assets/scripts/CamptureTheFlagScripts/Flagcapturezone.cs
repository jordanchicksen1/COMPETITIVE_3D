using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FlagCaptureZone : MonoBehaviour
{
    [Tooltip("The team this base belongs to. A capture only counts if the carried flag's TeamId is different from this.")]
    public int TeamId = 0;

    public delegate void FlagCaptured(int scoringTeamId, GameObject scoringPlayer);
    public event FlagCaptured OnFlagCaptured;

    void OnTriggerEnter(Collider other)
    {
        FlagPlayerController player = other.GetComponentInParent<FlagPlayerController>();
        if (player == null || !player.HasFlag)
        {
            return;
        }

        Flag carriedFlag = player.CarriedFlag;

        // Only counts as a capture if it's the enemy's flag, not your own.
        if (carriedFlag.TeamId == TeamId)
        {
            return;
        }

        carriedFlag.ReturnToBase();
        OnFlagCaptured?.Invoke(TeamId, player.gameObject);
    }
}