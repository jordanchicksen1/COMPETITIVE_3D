using UnityEngine;
using UnityEngine.InputSystem;

public class ScoreboardUi : MonoBehaviour
{
    [Tooltip("Auto-found if left empty.")]
    public PlayerInputManager playerInputManager;

    [Tooltip("Reads scores from here.")]
    public FlagScoreManager scoreManager;

    [Tooltip("One entry per possible player (2-4), in join order. Start these INACTIVE in the Editor.")]
    public ScoreboardEntryUi[] entries;

    void Awake()
    {
        if (entries != null)
        {
            foreach (ScoreboardEntryUi entry in entries)
            {
                if (entry != null)
                {
                    entry.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnEnable()
    {
        if (playerInputManager == null)
        {
            playerInputManager = FindObjectOfType<PlayerInputManager>();
        }

        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += HandlePlayerJoined;
        }
        else
        {
            Debug.LogWarning("ScoreboardUI: no PlayerInputManager found in the scene.");
        }

        FlagPlayerController[] existingPlayers = FindObjectsOfType<FlagPlayerController>();
        foreach (FlagPlayerController player in existingPlayers)
        {
            ShowEntryFor(player.TeamId - 1);
        }

        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
        }
        else
        {
            Debug.LogWarning("ScoreboardUI: no FlagScoreManager assigned.");
        }
    }

    void OnDisable()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= HandlePlayerJoined;
        }

        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        ShowEntryFor(playerInput.playerIndex);
    }

    private void ShowEntryFor(int playerIndex)
    {
        if (entries == null || playerIndex < 0 || playerIndex >= entries.Length)
        {
            return;
        }

        ScoreboardEntryUi entry = entries[playerIndex];
        if (entry == null)
        {
            return;
        }

        entry.gameObject.SetActive(true);
        entry.SetPlayer(playerIndex);
    }

    private void HandleScoreChanged(int playerIndex, float newScore)
    {
        if (entries == null || playerIndex < 0 || playerIndex >= entries.Length)
        {
            return;
        }

        ScoreboardEntryUi entry = entries[playerIndex];
        if (entry != null)
        {
            entry.SetScore(newScore);
        }
    }
}