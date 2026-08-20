using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    [Header("End Game Buttons")]
    [Tooltip("Button used to restart the game.")]
    public Button restartButton;

    [Tooltip("Button used to return to the menu.")]
    public Button menuButton;

    [Header("Menu")]
    [Tooltip("Exact name of your menu scene.")]
    public string menuSceneName = "MainMenu";

    private bool panelActive = false;

    public void Show(
        List<int> winningPlayerIndices,
        float winningScore
    )
    {
        Debug.Log("=== SHOWING WINNER PANEL ===");

        gameObject.SetActive(true);

        panelActive = true;

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);

            titleText.text =
                winningPlayerIndices.Count > 1
                ? "IT'S A TIE!"
                : "WE HAVE A WINNER!";

            titleText.color = Color.white;
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);

            scoreText.text =
                $"{Mathf.FloorToInt(winningScore)} points";

            scoreText.color = Color.white;
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

        if (winnerEntryPrefab != null &&
            winnerEntriesContainer != null)
        {
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

        SelectRestartButton();

        Debug.Log("=== WINNER PANEL FINISHED ===");
    }

    private void SelectRestartButton()
    {
        if (restartButton == null)
        {
            Debug.LogWarning(
                "WinnerPanelUI: Restart Button is not assigned."
            );

            return;
        }

        restartButton.gameObject.SetActive(true);
        restartButton.interactable = true;

        EventSystem.current?.SetSelectedGameObject(null);

        EventSystem.current?.SetSelectedGameObject(
            restartButton.gameObject
        );

        restartButton.Select();

        Debug.Log("Restart button selected.");
    }

    public void RestartGame()
    {
        Debug.Log("Restart Game pressed.");

        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    public void ReturnToMenu()
    {
        Debug.Log("Return To Menu pressed.");

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            menuSceneName
        );
    }


    private void Update()
    {
        if (!panelActive)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ReturnToMenu();
        }
    }
}