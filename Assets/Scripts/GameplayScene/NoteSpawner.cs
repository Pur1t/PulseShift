using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Static Lane Positions")]
    public RectTransform[] laneSpawns;    // Four static spawn positions (assign these in the Inspector)
    public RectTransform[] laneReceptors; // Four static receptor positions (assign these in the Inspector)

    [Header("Note Prefabs")]
    public GameObject[] notePrefabs;  // Array of note prefabs, one for each lane

    [Header("Timing")]
    public float travelTime = 1.5f;   // Duration for a note to travel from spawn to receptor

    private Beatmap currentBeatmap;
    private int nextNoteIndex = 0;
    private RectTransform canvasTransform;

    void Start()
    {
        // Automatically find the Canvas
        canvasTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();
    }

    // Call this method after the beatmap is loaded (for example, from your GameManager).
    public void SetBeatmap(Beatmap beatmap)
    {
        currentBeatmap = beatmap;
        nextNoteIndex = 0;
        Debug.Log("NoteSpawner: Beatmap set with " + beatmap.HitObjects.Count + " hit objects.");
    }

    void Update()
    {
        // Only spawn notes if the game has started.
        if (PauseManager.Instance != null && !PauseManager.Instance.gameStarted)
            return;

        if (currentBeatmap == null || nextNoteIndex >= currentBeatmap.HitObjects.Count)
            return;

        HitObject nextHit = currentBeatmap.HitObjects[nextNoteIndex];
        // Spawn the note when the song position reaches (hit.Time - travelTime).
        if (Conductor.Instance.songPosition >= nextHit.Time - travelTime)
        {
            SpawnNote(nextHit);
            nextNoteIndex++;
        }
    }


    void SpawnNote(HitObject hit)
    {
        if (hit.Lane < 0 || hit.Lane >= laneSpawns.Length)
        {
            Debug.LogWarning("NoteSpawner: Invalid lane index: " + hit.Lane);
            return;
        }

        Transform spawnPos = laneSpawns[hit.Lane];
        Transform receptorPos = laneReceptors[hit.Lane];

        if (notePrefabs == null || notePrefabs.Length <= hit.Lane)
        {
            Debug.LogError("NoteSpawner: Note prefab for lane " + hit.Lane + " is not assigned.");
            return;
        }

        // Debug: Check the expected positions.
        //Debug.Log("Spawning note for lane " + hit.Lane +
        //    " at spawnPos: " + spawnPos.position +
        //    " with receptorPos: " + receptorPos.position +
        //    " and target time: " + hit.Time);

        // Instantiate the appropriate note prefab at the static spawn position.
        GameObject noteObject = Instantiate(notePrefabs[hit.Lane], spawnPos);
        Note noteScript = noteObject.GetComponent<Note>();
        noteScript.lane = hit.Lane;
        noteScript.targetTime = hit.Time;  // (in seconds)
        noteScript.targetPosition = receptorPos;
        noteScript.travelTime = travelTime;

        // Register this note with the JudgeSystem
        JudgeSystem.Instance.RegisterNote(noteScript);
    }
}