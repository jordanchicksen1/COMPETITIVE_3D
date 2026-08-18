using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ModeSelectManager : MonoBehaviour
{
    [Tooltip("Mode cards, left to right in the order they're displayed.")]
    public GameModeCardUI[] cards;

    [Tooltip("The RectTransform holding all the cards (the one with the Horizontal Layout Group). This is what actually slides.")]
    public RectTransform content;

    [Tooltip("How quickly the carousel slides to the newly selected card.")]
    public float slideSpeed = 10f;

    [Tooltip("Delay between repeat moves while holding a direction, so one tap = one move.")]
    public float moveRepeatDelay = 0.2f;

    private int selectedIndex;
    private float nextMoveAllowedTime;
    private float targetX;

    void Start()
    {
        selectedIndex = 0;
        RefreshSelection();
        RecalculateTarget();

        if (content != null)
        {
            Vector2 pos = content.anchoredPosition;
            pos.x = targetX;
            content.anchoredPosition = pos;
        }
    }

    void Update()
    {
        HandleInput();

        if (content == null) return;

        Vector2 pos = content.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, targetX, slideSpeed * Time.deltaTime);
        content.anchoredPosition = pos;
    }

    private void HandleInput()
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
        RecalculateTarget();
    }

    private void RecalculateTarget()
    {
        if (cards == null || cards.Length == 0 || cards[selectedIndex] == null)
        {
            return;
        }

        RectTransform selectedRect = cards[selectedIndex].GetComponent<RectTransform>();
        targetX = -selectedRect.anchoredPosition.x;
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