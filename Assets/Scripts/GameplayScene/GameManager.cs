using UnityEngine;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static Conductor Instance;
    public TextMeshProUGUI startPrompt;  // Assign in Inspector (e.g. a UI Text element)
    private GameObject startPromptParent;
    public BeatmapLoader beatmapLoader;    // Reference to the BeatmapLoader (could be on the same GameObject)
    public NoteSpawner noteSpawner;        // Reference to the NoteSpawner in the scene
    public Conductor conductor;            // Reference to the Conductor

    private bool gameStarted = false;

    void Start()
    {
        startPromptParent = startPrompt.transform.parent.gameObject;
        // Load beatmap using SelectedSongData.chartFilePath.
        Beatmap beatmap = beatmapLoader.LoadBeatmap(SelectedSongData.chartFilePath);
        if (beatmap != null)
        {
            noteSpawner.SetBeatmap(beatmap);

            int totalNotes = beatmap.HitObjects.Count;
            ScoreSystem.Instance.totalNotes = totalNotes;
            AccuracySystem.Instance.SetTotalNotes(totalNotes);

        }
        else
        {
            Debug.LogError("Failed to load beatmap.");
        }

        startPrompt.text = "Press ENTER to start\nHold 'P' to Pause";
        startPrompt.gameObject.SetActive(true);
        startPromptParent.gameObject.SetActive(true);
    }

    void Update()
    {
        // Pre-start: Wait for Enter to start the game.
        if (!gameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            gameStarted = true;
            startPromptParent.gameObject.SetActive(false);
            startPrompt.gameObject.SetActive(false);

            // Start audio playback.
            conductor.PlaySong();
            conductor.hasStartedPlaying = true;
        }
    }
}