using UnityEngine;
using TMPro;


public class ScoreboardEntryUi : MonoBehaviour
{
    [SerializeField] private TMP_Text playerLabel;
    [SerializeField] private TMP_Text scoreLabel;

    public void SetPlayer(int playerIndex)
    {
        if (playerLabel != null)
        {
            playerLabel.text = $"Player {playerIndex + 1}";
        }

        SetScore(0f);
    }

    public void SetScore(float score)
    {
        if (scoreLabel != null)
        {
            scoreLabel.text = Mathf.FloorToInt(score).ToString();
        }
    }
}