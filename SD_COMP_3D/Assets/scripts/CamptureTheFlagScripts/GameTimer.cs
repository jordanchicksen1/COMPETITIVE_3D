using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Match Settings")]
    [Tooltip("Match length in seconds. 300 = 5 minutes.")]
    public float matchDuration = 300f;

    [Header("UI")]
    public TMP_Text timerText;

    [Tooltip("The score manager containing the final player scores.")]
    public FlagScoreManager scoreManager;

    [Tooltip("The winner panel shown when the match ends.")]
    public WinnerPanelUI winnerPanel;

    public bool GameEnded { get; private set; }

    public bool TimerRunning { get; private set; }

    private float remaining;

    private void Start()
    {
        remaining = matchDuration;
        GameEnded = false;
        TimerRunning = false;

        UpdateTimerText();

        Debug.Log(
            "GameTimer ready. Match duration: " + matchDuration +
            " (waiting for StartTimer()).");
    }

    public void StartTimer()
    {
        if (TimerRunning || GameEnded)
        {
            return;
        }

        remaining = matchDuration;
        TimerRunning = true;

        UpdateTimerText();

        Debug.Log("GameTimer started. Match duration: " + matchDuration);
    }

    private void Update()
    {
        if (!TimerRunning || GameEnded)
        {
            return;
        }

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateTimerText();
            EndGame();
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        if (GameEnded)
            return;

        GameEnded = true;
        TimerRunning = false;

        Debug.Log("========== GAME ENDED ==========");

        FlagPlayerController[] players =
            FindObjectsOfType<FlagPlayerController>();

        Debug.Log("Players found: " + players.Length);

        foreach (FlagPlayerController player in players)
        {
            if (player != null)
            {
                player.SetGameplayEnabled(false);
            }
        }

        if (scoreManager == null)
        {
            Debug.LogError(
                "GameTimer ERROR: Score Manager is NOT assigned!"
            );
            return;
        }

        if (winnerPanel == null)
        {
            Debug.LogError(
                "GameTimer ERROR: Winner Panel is NOT assigned!"
            );
            return;
        }

        if (players.Length == 0)
        {
            Debug.LogError(
                "GameTimer ERROR: No FlagPlayerController objects were found!"
            );
            return;
        }

        List<int> winningPlayerIndices = new List<int>();
        float highestScore = float.MinValue;

        foreach (FlagPlayerController player in players)
        {
            if (player == null)
                continue;

            int playerIndex = player.TeamId - 1;
            float score = scoreManager.GetScore(playerIndex);

            Debug.Log(
                $"Player {playerIndex + 1} score = {score}"
            );

            if (score > highestScore)
            {
                highestScore = score;
                winningPlayerIndices.Clear();
                winningPlayerIndices.Add(playerIndex);
            }
            else if (Mathf.Approximately(score, highestScore))
            {
                winningPlayerIndices.Add(playerIndex);
            }
        }

        Debug.Log(
            $"Winner score: {highestScore}"
        );
        Debug.Log(
            $"Winning players: {winningPlayerIndices.Count}"
        );

        winnerPanel.Show(
            winningPlayerIndices,
            highestScore
        );

        Debug.Log("WinnerPanel.Show() called successfully.");
    }
}