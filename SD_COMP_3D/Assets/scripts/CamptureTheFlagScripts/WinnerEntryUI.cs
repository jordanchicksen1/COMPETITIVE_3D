using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinnerEntryUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;

    public void Setup(int playerIndex, Color playerColor)
    {
        gameObject.SetActive(true);


        if (label != null)
        {
            label.gameObject.SetActive(true);

            label.text = $"Player {playerIndex + 1}";

            label.color = Color.white;

            Debug.Log(
                "Winner label created: " +
                label.text
            );
        }
        else
        {
            Debug.LogError(
                "WinnerEntryUI: LABEL IS NOT ASSIGNED!"
            );
        }
    }
}