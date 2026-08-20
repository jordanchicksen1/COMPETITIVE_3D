using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WinnerPanelUI : MonoBehaviour
{
    [Header("Player Colours")]
    public List<Color> playerColours = new List<Color>
    {
        Color.green,
        Color.red,
        Color.blue,
        Color.yellow
    };

    [Header("Winner Text")]
    public TMP_Text titleText;
    public TMP_Text scoreText;

    [Header("Winner Entries")]
    public WinnerEntryUI winnerEntryPrefab;
    public Transform winnerEntriesContainer;

    public void Show(List<int> winningPlayerIndices, float winningScore)
    {
  
        gameObject.SetActive(true);

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);

            titleText.text =
                winningPlayerIndices.Count > 1
                ? "IT'S A TIE!"
                : "WE HAVE A WINNER!";

            titleText.color = Color.white;

            Debug.Log("TITLE: " + titleText.text);
        }
        else
        {
            Debug.LogError("TITLE TEXT IS NOT ASSIGNED!");
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);

            scoreText.text =
                $"{Mathf.FloorToInt(winningScore)} points";

            scoreText.color = Color.white;

            Debug.Log("SCORE: " + scoreText.text);
        }
        else
        {
            Debug.LogError("SCORE TEXT IS NOT ASSIGNED!");
        }

        if (winnerEntriesContainer != null)
        {
            for (int i = winnerEntriesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(
                    winnerEntriesContainer.GetChild(i).gameObject
                );
            }
        }


        if (winnerEntryPrefab == null)
        {
            Debug.LogError("WINNER ENTRY PREFAB IS NOT ASSIGNED!");
            return;
        }

        if (winnerEntriesContainer == null)
        {
            Debug.LogError(
                "WINNER ENTRIES CONTAINER IS NOT ASSIGNED!"
            );
            return;
        }

        foreach (int playerIndex in winningPlayerIndices)
        {
            WinnerEntryUI entry =
                Instantiate(
                    winnerEntryPrefab,
                    winnerEntriesContainer
                );

            entry.gameObject.SetActive(true);

            Color playerColor = Color.white;

            if (playerIndex >= 0 &&
                playerIndex < playerColours.Count)
            {
                playerColor = playerColours[playerIndex];
            }

            entry.Setup(
                playerIndex,
                playerColor
            );
        }

    }
}