using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeCardUI : MonoBehaviour
{
    [Tooltip("Shown on the card.")]
    public string displayName = "Game Mode";

    [Tooltip("Exact name of the scene to load when this card is selected and confirmed. Must be added to Build Settings.")]
    public string sceneName;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image background;
    [SerializeField] private GameObject selectedHighlight;

    [Tooltip("How much bigger the card gets when selected (1 = no change).")]
    public float selectedScale = 1.15f;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;

        if (titleText != null)
        {
            titleText.text = displayName;
        }
    }

    public void SetSelected(bool isSelected)
    {
        transform.localScale = isSelected ? baseScale * selectedScale : baseScale;

        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(isSelected);
        }

        if (background != null)
        {
            Color c = background.color;
            c.a = isSelected ? 1f : 0.6f;
            background.color = c;
        }
    }
}