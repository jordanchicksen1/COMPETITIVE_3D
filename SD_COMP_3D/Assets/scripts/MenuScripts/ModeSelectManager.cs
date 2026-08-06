using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// TRUE SLIDING CAROUSEL setup:
// - "Viewport": a RectTransform with a RectMask2D component, sized to show
//   however much of the row you want visible (e.g. 3 cards' worth wide).
//   Anchored/pivoted at its own center (0.5, 0.5) and positioned where you
//   want the carousel to sit on screen.
// - "Content": a child of Viewport, anchored/pivoted at (0, 0.5) - left-
//   middle. Has a Horizontal Layout Group + Content Size Fitter (Horizontal
//   Fit = Preferred Size) so it grows to fit however many cards you add.
// - Cards: children of Content, each with GameModeCardUI + a Layout
//   Element for fixed size.
//
// This script SLIDES Content left/right so whichever card is selected
// lands centered inside Viewport - it reads each card's real position
// rather than assuming a fixed width/spacing, so it works regardless of
// card size or how many cards you have.
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

        // Snap instantly to the starting position instead of sliding in
        // from wherever Content happened to be left in the Editor.
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

    // Reads the selected card's own position inside Content (as laid out
    // by the Horizontal Layout Group) and targets shifting Content by
    // exactly the negative of that - so the selected card ends up at
    // Content's parent's local zero, i.e. centered in the Viewport.
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