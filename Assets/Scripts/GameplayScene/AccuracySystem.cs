using UnityEngine;
using TMPro;

public class AccuracySystem : MonoBehaviour
{
    public static AccuracySystem Instance;

    public TextMeshProUGUI accuracyText;  // UI element to display accuracy
    public float currentAccuracy = 100f;

    private int totalNotes = 0;           // Total notes in the beatmap (set externally)
    private int judgedCount = 0;          // How many notes have been judged
    private float totalJudgementPoints = 0f; // Sum of awarded points (Perfect = 1, Great = 0.8, Miss = 0)

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Call this when the beatmap is loaded.
    public void SetTotalNotes(int count)
    {
        totalNotes = count;
        judgedCount = 0;
        totalJudgementPoints = 0f;
        UpdateAccuracyUI();
    }

    // Report a judgement for one note.
    // Perfect = 1.0, Great = 0.8, Miss = 0.
    public void ReportJudgement(JudgementType judgement)
    {
        judgedCount++;
        if (judgement == JudgementType.Perfect)
            totalJudgementPoints += 1f;
        else if (judgement == JudgementType.Great)
            totalJudgementPoints += 0.8f;
        else if (judgement == JudgementType.Miss)
            totalJudgementPoints += 0f;
        UpdateAccuracyUI();
    }

    private void UpdateAccuracyUI()
    {
        if (judgedCount > 0)
        {
            currentAccuracy = (totalJudgementPoints / judgedCount) * 100f;
        }
        accuracyText.text = currentAccuracy.ToString("F2");
    }
}