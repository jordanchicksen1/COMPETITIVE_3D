using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FlagCaptureZone : MonoBehaviour
{
    [Tooltip("The team this base/zone belongs to.")]
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

        if (player.TeamId != TeamId)
        {
            return;
        }

        Flag carriedFlag = player.CarriedFlag;
        carriedFlag.ReturnToBase();
        player.ClearHeldFlag();
        OnFlagCaptured?.Invoke(TeamId, player.gameObject);
    }
}