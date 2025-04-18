using UnityEngine;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance;

    public TextMeshProUGUI scoreText;  // UI element for the score display

    public int score = 0;             // Actual score (integer)
    
    private float displayedScore = 0f; // For the running number effect
    public float scoreSpeed = 15000f * 50f;    // Speed at which the displayed score catches up

    [HideInInspector]
    public int totalNotes = 0;          // Total number of notes in the beatmap (set externally)

    // The value of each perfect hit such that perfect for all notes yields 1,000,000 points.
    public float PerfectValue
    {
        get { return totalNotes > 0 ? 1000000f / totalNotes : 0; }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Call this method to add score (amount may be fractional, but we round for display)
    public void AddScore(float amount)
    {
        score += Mathf.RoundToInt(amount);
    }

    void Update()
    {
        if (displayedScore < score)
        {
            displayedScore = Mathf.MoveTowards(displayedScore, score, scoreSpeed * Time.deltaTime * 10000);
            scoreText.text = Mathf.FloorToInt(displayedScore).ToString("N0");  // Format with commas, e.g. "123,456"
        }
        else
        {
            scoreText.text = score.ToString("N0");
        }
    }

    // Optionally, if you need to save the score at song end:
    public void SaveScore()
    {
        // Save score logic (e.g., add to leaderboard)
    }
}
