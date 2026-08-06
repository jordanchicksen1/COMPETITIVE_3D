using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ModeSelectManager : MonoBehaviour
{
    [Tooltip("Mode cards, left to right in the order they're displayed.")]
    public GameModeCardUI[] cards;

    [Tooltip("Delay between repeat moves while holding a direction, so one tap = one move.")]
    public float moveRepeatDelay = 0.2f;

    private int selectedIndex;
    private float nextMoveAllowedTime;

    void Start()
    {
        selectedIndex = 0;
        RefreshSelection();
    }

    void Update()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null || cards == null || cards.Length == 0)
        {
            return;
        }

        if (Time.time >= nextMoveAllowedTime)
        {
            if (gamepad.dpad.left.wasPressedThisFrame || gamepad.leftStick.left.wasPressedThisFrame)
            {
                Move(-1);
            }
            else if (gamepad.dpad.right.wasPressedThisFrame || gamepad.leftStick.right.wasPressedThisFrame)
            {
                Move(1);
            }
        }

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    private void Move(int direction)
    {
        selectedIndex = Mathf.Clamp(selectedIndex + direction, 0, cards.Length - 1);
        nextMoveAllowedTime = Time.time + moveRepeatDelay;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                cards[i].SetSelected(i == selectedIndex);
            }
        }
    }

    private void ConfirmSelection()
    {
        if (selectedIndex < 0 || selectedIndex >= cards.Length) return;

        GameModeCardUI selectedCard = cards[selectedIndex];
        if (selectedCard == null || string.IsNullOrEmpty(selectedCard.sceneName))
        {
            Debug.LogWarning("ModeSelectManager: selected card has no scene name set.");
            return;
        }

        SceneManager.LoadScene(selectedCard.sceneName);
    }
}