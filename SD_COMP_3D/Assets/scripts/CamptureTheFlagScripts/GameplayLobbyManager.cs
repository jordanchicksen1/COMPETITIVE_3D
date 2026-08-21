using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class GameLobbyManager : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag the PlayerInputManager in your scene here.")]
    public PlayerInputManager playerInputManager;

    [Tooltip("The Start button shown in the lobby UI. Assign its OnClick to GameLobbyManager.StartGame().")]
    public Button startButton;

    [Tooltip("Centered text that guides players through joining, starting, and the countdown.")]
    public TMP_Text centerText;

    [Tooltip("Minimum number of players required before Start can be pressed. Set to 1 if any single player should be able to start.")]
    public int minPlayersToStart = 1;

    [Tooltip("The match timer. StartTimer() is called on it once the countdown finishes.")]
    public GameTimer gameTimer;

    [Tooltip("Seconds shown counting down after Start is pressed, before gameplay unlocks.")]
    public int countdownSeconds = 3;

    [Header("Messages")]
    public string joinPromptText = "Press Any Button to Join";
    public string startPromptText = "Press Start";

    private readonly List<FlagPlayerController> joinedPlayers = new List<FlagPlayerController>();
    private readonly List<PlayerInput> joinedPlayerInputs = new List<PlayerInput>();

    private bool gameStarted;
    private bool startSequenceBegun;

    void Awake()
    {
        if (playerInputManager == null)
        {
            playerInputManager = FindObjectOfType<PlayerInputManager>();
        }

        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += HandlePlayerJoined;
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
            startButton.onClick.AddListener(StartGame);
        }

        UpdateCenterText();
    }

    void OnDestroy()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= HandlePlayerJoined;
        }
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        if (gameStarted || startSequenceBegun)
        {
            return;
        }

        FlagPlayerController controller =
            playerInput.GetComponent<FlagPlayerController>();

        if (controller == null)
        {
            Debug.LogWarning(
                "GameLobbyManager: joined PlayerInput has no FlagPlayerController."
            );
            return;
        }

        controller.SetGameplayEnabled(false);

        joinedPlayers.Add(controller);
        joinedPlayerInputs.Add(playerInput);

        UpdateStartButtonVisibility();
        UpdateCenterText();
    }

    void Update()
    {
        if (gameStarted || startSequenceBegun)
        {
            return;
        }

        if (joinedPlayers.Count < minPlayersToStart)
        {
            return;
        }

        foreach (PlayerInput input in joinedPlayerInputs)
        {
            if (input == null)
            {
                continue;
            }

            foreach (InputDevice device in input.devices)
            {
                if (device is Gamepad gamepad &&
                    gamepad.buttonNorth.wasPressedThisFrame)
                {
                    StartGame();
                    return;
                }
            }
        }
    }

    private void UpdateCenterText()
    {
        if (centerText == null)
        {
            return;
        }

        if (startSequenceBegun)
        {
            return;
        }

        centerText.text =
            joinedPlayers.Count >= minPlayersToStart
                ? startPromptText
                : joinPromptText;
    }

    private void UpdateStartButtonVisibility()
    {
        if (startButton == null)
        {
            return;
        }

        bool canStart =
            !gameStarted && !startSequenceBegun && joinedPlayers.Count >= minPlayersToStart;

        startButton.gameObject.SetActive(canStart);
    }

    public void StartGame()
    {
        if (gameStarted || startSequenceBegun)
        {
            return;
        }

        if (joinedPlayers.Count < minPlayersToStart)
        {
            return;
        }

        startSequenceBegun = true;

        if (playerInputManager != null)
        {
            playerInputManager.DisableJoining();
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
        }

        StartCoroutine(CountdownAndBeginMatch());
    }

    private IEnumerator CountdownAndBeginMatch()
    {
        for (int secondsLeft = countdownSeconds; secondsLeft > 0; secondsLeft--)
        {
            if (centerText != null)
            {
                centerText.text = secondsLeft.ToString();
            }

            yield return new WaitForSeconds(1f);
        }

        if (centerText != null)
        {
            centerText.text = string.Empty;
        }

        BeginMatch();
    }

    private void BeginMatch()
    {
        gameStarted = true;

        foreach (FlagPlayerController controller in joinedPlayers)
        {
            if (controller != null)
            {
                controller.SetGameplayEnabled(true);
            }
        }

        if (gameTimer != null)
        {
            gameTimer.StartTimer();
        }
        else
        {
            Debug.LogWarning(
                "GameLobbyManager: no GameTimer assigned, match timer will not start."
            );
        }
    }

    public bool HasGameStarted()
    {
        return gameStarted;
    }

    public IReadOnlyList<FlagPlayerController> JoinedPlayers => joinedPlayers;
}