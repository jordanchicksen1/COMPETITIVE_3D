using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FlagCaptureZone : MonoBehaviour
{
    [Tooltip("The team this zone belongs to. Only this team's flag carrier scores points here.")]
    public int TeamId = 0;

    [Tooltip("Points awarded per second while the team's flag carrier holds this zone.")]
    public float pointsPerSecond = 1f;

    public float TeamScore { get; private set; }

    public delegate void ScoreChanged(int teamId, float newScore);
    public event ScoreChanged OnScoreChanged;

    public delegate void HoldStateChanged(int teamId, bool isBeingHeld);
    public event HoldStateChanged OnHoldStateChanged;

    private readonly HashSet<FlagPlayerController> playersInZone = new HashSet<FlagPlayerController>();
    private bool wasBeingHeld;

    void OnTriggerEnter(Collider other)
    {
        FlagPlayerController player = other.GetComponentInParent<FlagPlayerController>();
        if (player != null)
        {
            playersInZone.Add(player);
        }
    }

    void OnTriggerExit(Collider other)
    {
        FlagPlayerController player = other.GetComponentInParent<FlagPlayerController>();
        if (player != null)
        {
            playersInZone.Remove(player);
        }
    }

    void Update()
    {
        bool isBeingHeld = false;

        playersInZone.RemoveWhere(p => p == null);

        foreach (FlagPlayerController player in playersInZone)
        {
            if (player.TeamId == TeamId && player.HasFlag)
            {
                isBeingHeld = true;
                break; 
            }
        }

        if (isBeingHeld)
        {
            TeamScore += pointsPerSecond * Time.deltaTime;
            OnScoreChanged?.Invoke(TeamId, TeamScore);
        }

        if (isBeingHeld != wasBeingHeld)
        {
            wasBeingHeld = isBeingHeld;
            OnHoldStateChanged?.Invoke(TeamId, isBeingHeld);
        }
    }
}