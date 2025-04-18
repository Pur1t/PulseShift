using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class JudgeSystem : MonoBehaviour
{
    public static JudgeSystem Instance;

    [Header("Judgement Timings (ms)")]
    public float perfectWindow = 50f;
    public float greatWindow = 100f;
    public float missWindow = 150f;

    private GameObject perfectDisplay;
    private GameObject greatDisplay;
    private GameObject missDisplay;

    // List of active notes in the scene
    private List<Note> activeNotes = new List<Note>();
    private Coroutine judgementCoroutine;

    void Awake()
    {
        // Find the judgement display GameObjects by name.
        perfectDisplay = GameObject.Find("PerfectJudgement");
        greatDisplay = GameObject.Find("GreatJudgement");
        missDisplay = GameObject.Find("MissJudgement");

        if (perfectDisplay != null) perfectDisplay.SetActive(false);
        if (greatDisplay != null) greatDisplay.SetActive(false);
        if (missDisplay != null) missDisplay.SetActive(false);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InputManager.OnKeyPress += CheckHit;
    }

    void OnDestroy()
    {
        InputManager.OnKeyPress -= CheckHit;
    }

    // Call this from NoteSpawner when a note is spawned
    public void RegisterNote(Note note)
    {
        activeNotes.Add(note);
    }

    // Called when the player presses a key; lane indicates which lane was pressed.
    void CheckHit(int lane)
    {
        // Skip input processing if game is paused.
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            return;

        if (activeNotes == null || activeNotes.Count == 0)
        {
            Debug.LogWarning("No active notes to check.");
            return;
        }

        Note closestNote = null;
        float closestError = float.MaxValue;

        // Loop through active notes and find the one in the pressed lane with the smallest absolute timing error.
        foreach (var note in activeNotes)
        {
            if (note.lane == lane)
            {
                // Calculate error: negative means pressed early, positive means pressed late.
                float error = Conductor.Instance.songPosition - note.targetTime;
                if (Mathf.Abs(error) < Mathf.Abs(closestError))
                {
                    closestError = error;
                    closestNote = note;
                }
            }
        }

        if (closestNote == null)
        {
            Debug.LogWarning("No note found in lane: " + lane);
            return;
        }

        // Check if the note has reached the earliest allowed hit window.
        // For example, allow input only if current time is later than (targetTime - greatWindow)
        float earliestAllowedTime = closestNote.targetTime - (greatWindow / 1000f);
        if (Conductor.Instance.songPosition < earliestAllowedTime)
        {
            Debug.Log("Pressed too early; ignoring input.");
            return;
        }

        // Judge the hit based on the timing error.
        JudgeHit(closestError, closestNote);
    }

    void JudgeHit(float timingError, Note note)
    {
        if (note == null)
        {
            Debug.LogError("JudgeHit called with a null note!");
            return;
        }
        if (ScoreSystem.Instance == null || ComboSystem.Instance == null || AccuracySystem.Instance == null)
        {
            Debug.LogError("One of the required systems (Score, Combo, Health, Accuracy) is not initialized.");
            return;
        }

        float absError = Mathf.Abs(timingError);
        float errorInMs = absError * 1000f; // convert seconds error to ms

        if (errorInMs <= perfectWindow)
        {
            Debug.Log("Perfect!");
            ScoreSystem.Instance.AddScore(ScoreSystem.Instance.PerfectValue);
            ComboSystem.Instance.IncreaseCombo();
            //HealthSystem.Instance.ModifyHealth(JudgementType.Perfect);
            AccuracySystem.Instance.ReportJudgement(JudgementType.Perfect);
            ShowJudgementDisplay(JudgementType.Perfect);
        }
        else if (errorInMs <= greatWindow)
        {
            Debug.Log("Great!");
            ScoreSystem.Instance.AddScore(ScoreSystem.Instance.PerfectValue * 0.8f);
            ComboSystem.Instance.IncreaseCombo();
            //HealthSystem.Instance.ModifyHealth(JudgementType.Great);
            AccuracySystem.Instance.ReportJudgement(JudgementType.Great);
            ShowJudgementDisplay(JudgementType.Great);
        }
        else if (errorInMs <= missWindow)
        {
            Debug.Log("Miss!");
            ComboSystem.Instance.ResetCombo();
            //HealthSystem.Instance.ModifyHealth(JudgementType.Miss);
            AccuracySystem.Instance.ReportJudgement(JudgementType.Miss);
            ShowJudgementDisplay(JudgementType.Miss);
        }

        activeNotes.Remove(note);
        Destroy(note.gameObject);
    }

    // Automatically check for notes that have passed their miss window.
    void Update()
    {
        // Do nothing if game is paused.
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            return;

        if (activeNotes == null || activeNotes.Count == 0)
            return;

        float currentTime = Conductor.Instance.songPosition;
        // Loop backwards to safely remove items from the list.
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            float error = currentTime - note.targetTime; // positive if note is late
            if (error > missWindow / 1000f) // if beyond the miss window
            {
                Debug.Log("Miss! (Auto)");
                ComboSystem.Instance.ResetCombo();
                AccuracySystem.Instance.ReportJudgement(JudgementType.Miss);
                ShowJudgementDisplay(JudgementType.Miss);
                activeNotes.RemoveAt(i);
                Destroy(note.gameObject);
            }
        }
    }

    // New method to show judgement UI for 0.5 seconds.
    private void ShowJudgementDisplay(JudgementType judgement)
    {
        // First, disable all judgement displays.
        if (perfectDisplay != null) perfectDisplay.SetActive(false);
        if (greatDisplay != null) greatDisplay.SetActive(false);
        if (missDisplay != null) missDisplay.SetActive(false);

        GameObject display = null;
        switch (judgement)
        {
            case JudgementType.Perfect:
                display = perfectDisplay;
                break;
            case JudgementType.Great:
                display = greatDisplay;
                break;
            case JudgementType.Miss:
                display = missDisplay;
                break;
        }

        if (display != null)
        {
            display.SetActive(true);
            if (judgementCoroutine != null)
                StopCoroutine(judgementCoroutine);
            judgementCoroutine = StartCoroutine(HideJudgementDisplayAfterDelay(display, 0.25f));
        }
    }

    private IEnumerator HideJudgementDisplayAfterDelay(GameObject display, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (display != null)
            display.SetActive(false);
        judgementCoroutine = null;
    }
}
