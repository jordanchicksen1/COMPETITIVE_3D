using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WinnerPanelUI : MonoBehaviour
{
    [Header("Player Winner Texts")]
    [Tooltip("Assign Player 1, Player 2, Player 3 and Player 4 text objects.")]
    public TMP_Text[] playerWinnerTexts = new TMP_Text[4];

    [Header("Winner Text")]
    public TMP_Text titleText;
    public TMP_Text scoreText;

    [Header("Buttons")]
    public Button restartButton;
    public Button menuButton;

    [Header("Menu")]
    public string menuSceneName = "MainMenu";

    private bool panelActive = false;

    public void Show(List<int> winningPlayerIndices, float winningScore)
    {
        gameObject.SetActive(true);

        panelActive = true;

        for (int i = 0; i < playerWinnerTexts.Length; i++)
        {
            if (playerWinnerTexts[i] != null)
            {
                playerWinnerTexts[i].gameObject.SetActive(false);
            }
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);

            titleText.text =
                winningPlayerIndices.Count > 1
                ? "IT'S A TIE!"
                : "WE HAVE A WINNER!";

        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = $"{Mathf.FloorToInt(winningScore)} points";
        }

        foreach (int playerIndex in winningPlayerIndices)
        {
            if (playerIndex >= 0 &&
                playerIndex < playerWinnerTexts.Length &&
                playerWinnerTexts[playerIndex] != null)
            {
                playerWinnerTexts[playerIndex].gameObject.SetActive(true);

                playerWinnerTexts[playerIndex].text =
                    $"PLAYER {playerIndex + 1}";
            }
        }

        SelectRestartButton();
    }

    private void SelectRestartButton()
    {
        if (restartButton == null)
            return;

        restartButton.gameObject.SetActive(true);
        restartButton.interactable = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            EventSystem.current.SetSelectedGameObject(
                restartButton.gameObject
            );
        }

        restartButton.Select();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }
   public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(menuSceneName);
    }

    private void Update()
    {
        if (!panelActive)
            return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartGame();
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReturnToMenu();
                return;
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                RestartGame();
                return;
            }

            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                ReturnToMenu();
                return;
            }
        }
    }
}