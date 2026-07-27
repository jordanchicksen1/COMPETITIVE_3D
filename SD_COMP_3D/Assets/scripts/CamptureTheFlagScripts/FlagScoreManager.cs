using UnityEngine;

public class FlagScoreManager : MonoBehaviour
{
    [Tooltip("The scene's flag object.")]
    public Flag flag;

    [Tooltip("Points awarded per second to whoever is currently holding the flag.")]
    public float pointsPerSecond = 1f;

    private readonly float[] scores = new float[4];

    public delegate void ScoreChanged(int playerIndex, float newScore);
    public event ScoreChanged OnScoreChanged;

    void Update()
    {
        if (flag == null || flag.State != Flag.FlagState.Carried || flag.CurrentCarrier == null)
        {
            return;
        }

        FlagPlayerController carrier = flag.CurrentCarrier.GetComponent<FlagPlayerController>();
        if (carrier == null)
        {
            return;
        }

        int index = carrier.TeamId - 1; 
        if (index < 0 || index >= scores.Length)
        {
            return;
        }

        scores[index] += pointsPerSecond * Time.deltaTime;
        OnScoreChanged?.Invoke(index, scores[index]);
    }

    public float GetScore(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= scores.Length)
        {
            return 0f;
        }

        return scores[playerIndex];
    }
}