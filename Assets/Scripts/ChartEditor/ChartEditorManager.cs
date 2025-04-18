using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NoteData
{
    // Raw time in seconds from the chart (e.g., 0.125, 0.25, etc.)
    public float time;
    // Lane index (0 to laneCount-1)
    public int lane;
}

public class ChartEditorManager : MonoBehaviour
{
    public static ChartEditorManager Instance;

    [Header("Note Setup")]
    public GameObject notePrefab;
    public RectTransform gridPanel;       // The Content of your ScrollRect
    public Canvas canvasRef;              // (May be unused if instantiating as child of gridPanel)
    public int laneCount = 4;
    public float laneWidth = 100f;        // (Not used in our calculation below; we use gridPanel width)

    [Header("Positioning Offsets")]
    public float verticalOffset = 113f;   // Y position for row 0
    public float rowHeight = 173f;        // Height per grid cell (per subdivision cell)
    public float horizontalOffset = 351f; // Horizontal adjustment

    [Header("Timing Settings")]
    public float bpm = 105f;              // BPM from chart (set manually in EditorConductor)
    // For a 1/4 subdivision, time per cell is computed as:
    // (60000 / BPM) gives ms per beat; then divide by 4 and convert to seconds.
    // Example: for BPM=120, (60000/120)=500 ms per beat, /4 = 125 ms = 0.125 sec.
    public int snapDivisor = 4; // default 4th notes
    public float timePerCell
    {
        get
        {
            return (60000f / bpm) / snapDivisor / 1000f;
        }
    }

    // Additional note offset (converted from ms to seconds)
    public float noteOffsetSec = 0.027f;

    [Header("Song Settings")]
    public float songDuration = 300f;     // Total song duration in seconds

    [Header("Chart File Settings")]
    public string chartFilePath = SelectedSongData.chartFilePath;          // Full path to chart.txt

    // List of hit objects loaded from chart file.
    public List<NoteData> chartNotes = new List<NoteData>();
    // List of instantiated note objects.
    public List<GameObject> instantiatedNotes = new List<GameObject>();

    public float duplicateThreshold = 0.01f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if you want it to persist across scenes.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        // Wait until EditorConductor.Instance and its AudioSource are ready
        while (EditorConductor.Instance == null ||
               EditorConductor.Instance.musicSource == null ||
               EditorConductor.Instance.musicSource.clip == null)
        {
            Debug.Log("Waiting for AudioSource and clip...");
            yield return null;
        }

        // Update songDuration based on the actual audio clip length.
        songDuration = EditorConductor.Instance.musicSource.clip.length;
        Debug.Log("Song duration set to: " + songDuration + " seconds.");

        // Add a buffer (15 seconds) to the effective song duration.
        float effectiveSongDuration = songDuration + 15f;

        // Calculate total rows using the effective duration (note: timePerCell is calculated from BPM and snap)
        int totalRows = Mathf.CeilToInt(effectiveSongDuration / timePerCell) + 1;
        float newHeight = verticalOffset + rowHeight * totalRows;
        gridPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        Debug.Log("Grid panel height set to: " + newHeight);

        LoadChart();
        InstantiateAllNotes();
    }

    /// <summary>
    /// Parses the chart file in the osu!-style format.
    /// It looks for the [HitObjects] section and parses each line.
    /// </summary>
    public void LoadChart()
    {
        string audioDirectory = Path.GetDirectoryName(SelectedSongData.chartFilePath);
        string chartAudioPath = Path.Combine(audioDirectory, "chart.txt");
        // If the provided path is absolute, use it directly.
        string fullPath = chartAudioPath;
        Debug.Log("Attempting to load chart from: " + fullPath);

        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            Debug.LogWarning("Chart file not found. Starting with an empty chart. Full path: " + fullPath);
            chartNotes = new List<NoteData>();
            return;
        }

        Debug.Log("Chart file exists at: " + fullPath);
        string[] lines = File.ReadAllLines(fullPath);

        // Parse the timing points and metadata
        ParseTimingPoints(lines, ref EditorConductor.Instance.metadata);

        // Now parse the [HitObjects] section.
        bool hitObjectsSection = false;
        chartNotes = new List<NoteData>();
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("[HitObjects]"))
            {
                hitObjectsSection = true;
                continue;
            }
            if (hitObjectsSection)
            {
                if (string.IsNullOrEmpty(trimmed))
                    continue;
                // Expect line format: "128,_ _ O _"
                string[] parts = trimmed.Split(',');
                if (parts.Length < 2)
                    continue;
                float timeMs;
                if (!float.TryParse(parts[0], out timeMs))
                    continue;
                float timeSec = timeMs / 1000f;
                string pattern = parts[1].Trim();
                string[] columns = pattern.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                int lane = -1;
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i] == "O")
                    {
                        lane = i;
                        break;
                    }
                }
                if (lane >= 0)
                {
                    NoteData nd = new NoteData { time = timeSec, lane = lane };
                    chartNotes.Add(nd);
                }
            }
        }
        Debug.Log("Loaded chart with " + chartNotes.Count + " hit objects.");
    }

    /// <summary>
    /// Saves the chart file in a simple osu!-style format (and score file).
    /// Only the [HitObjects] section is written.
    /// </summary>
    public void SaveChart()
    {
        string audioDirectory = Path.GetDirectoryName(SelectedSongData.audioFilePath);
        string scoreAudioPath = Path.Combine(audioDirectory, "score.txt");
        string chartAudioPath = Path.Combine(audioDirectory, "chart.txt");

        // Sort hit objects by time
        chartNotes.Sort((a, b) => a.time.CompareTo(b.time));

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Write Metadata section using metadata from EditorConductor
        if (EditorConductor.Instance != null && EditorConductor.Instance.metadata != null)
        {
            var meta = EditorConductor.Instance.metadata;
            sb.AppendLine("[Metadata]");
            sb.AppendLine("Artist: " + meta.artist);
            sb.AppendLine("Song: " + meta.songTitle);
            sb.AppendLine("Diff: -");
            sb.AppendLine("Chart Creator: -");
            sb.AppendLine("Source: -");
            sb.AppendLine();
        }
        else
        {
            // Fallback if metadata is missing.
            sb.AppendLine("[Metadata]");
            sb.AppendLine("Artist: Unknown");
            sb.AppendLine("Song: Unknown");
            sb.AppendLine("Diff: -");
            sb.AppendLine("Chart Creator: -");
            sb.AppendLine("Source: -");
            sb.AppendLine();
        }

        // Write TimingPoints section.
        // For this example, we write one timing point at time 0.
        // Calculate milliseconds per beat from BPM.
        float msPerBeat = 60000f / EditorConductor.Instance.metadata.bpm;
        sb.AppendLine("[TimingPoints]");
        sb.AppendLine("0," + msPerBeat.ToString("F6"));
        sb.AppendLine();

        // Write HitObjects section.
        sb.AppendLine("[HitObjects]");
        foreach (NoteData nd in chartNotes)
        {
            int timeMs = Mathf.RoundToInt(nd.time * 1000f);
            string pattern = "";
            for (int i = 0; i < laneCount; i++)
            {
                pattern += (i == nd.lane) ? "O" : "_";
                if (i < laneCount - 1)
                    pattern += " ";
            }
            sb.AppendLine(timeMs + "," + pattern);
        }

        File.WriteAllText(chartAudioPath, sb.ToString());
        Debug.Log("Chart saved to: " + chartAudioPath);

        // Also reset score (unchanged from before)
        string scoreContent = "[Score]\nScore: 0\nAccuracy: 00.00\nRank: X";
        string scorePath = Path.Combine(scoreAudioPath, "score.txt");
        if (!string.IsNullOrEmpty(SelectedSongData.audioFilePath))
        {
            File.WriteAllText(scoreAudioPath, scoreContent);
            Debug.Log("Score also saved in: " + scoreAudioPath);
        }
        else
        {
            Debug.LogError("Audio file path is not set. Could not save score in audio directory.");
        }
        SceneManager.LoadScene("MusicSelectedEditScene");
    }

    /// <summary>
    /// Instantiates all notes from chartNotes.
    /// </summary>
    void InstantiateAllNotes()
    {
        foreach (GameObject go in instantiatedNotes)
        {
            Destroy(go);
        }
        instantiatedNotes.Clear();
        foreach (NoteData nd in chartNotes)
        {
            InstantiateNote(nd);
        }
    }

    /// <summary>
    /// Instantiates a note as a child of the gridPanel.
    /// </summary>
    void InstantiateNote(NoteData noteData)
    {
        if (notePrefab == null)
        {
            Debug.LogError("Note prefab is not assigned!");
            return;
        }
        if (gridPanel == null)
        {
            Debug.LogError("Grid panel is not assigned!");
            return;
        }
        GameObject newNote = Instantiate(notePrefab, gridPanel);
        RectTransform rt = newNote.GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector2 pos = GetNotePosition(noteData.time, noteData.lane);
            rt.anchoredPosition = pos;
            rt.rotation = GetRotationForLane(noteData.lane);
        }
        else
        {
            Debug.LogError("Instantiated note lacks a RectTransform.");
        }
        // Add (or get) NoteInstance and assign its noteData.
        NoteInstance nInst = newNote.GetComponent<NoteInstance>();
        if (nInst == null)
        {
            nInst = newNote.AddComponent<NoteInstance>();
        }
        nInst.noteData = noteData;

        instantiatedNotes.Add(newNote);
    }

    public void RemoveNote(NoteData noteData, GameObject noteObj)
    {
        if (chartNotes.Contains(noteData))
        {
            chartNotes.Remove(noteData);
            Debug.Log("Removed NoteData from chartNotes.");
        }
        if (instantiatedNotes.Contains(noteObj))
        {
            instantiatedNotes.Remove(noteObj);
            Debug.Log("Removed GameObject from instantiatedNotes.");
        }
        Destroy(noteObj);
    }

    /// <summary>
    /// Computes the note position relative to the grid panel.
    /// Horizontal: Divide the grid panel's width (assumed fixed) into lanes and center the note in its lane.
    /// Vertical: Use the effective time which is (note.time + noteOffsetSec) to determine the row index:
    ///   rowIndex = (note.time + noteOffsetSec) / timePerCell
    /// Then: y = verticalOffset + rowHeight * rowIndex
    /// Finally, subtract horizontalOffset.
    /// </summary>
    public Vector2 GetNotePosition(float time, int lane)
    {
        float effectiveTime = time + noteOffsetSec;
        float rowIndex = effectiveTime / timePerCell;
        float panelWidth = gridPanel.rect.width;
        float cellWidth = panelWidth / laneCount;
        float x = lane * cellWidth + cellWidth / 2f;
        float y = verticalOffset + rowHeight * rowIndex;
        x -= horizontalOffset;
        Debug.Log("GetNotePosition: time=" + time + " (effectiveTime=" + effectiveTime + ", rowIndex=" + rowIndex + ") -> (" + x + ", " + y + ")");
        return new Vector2(x, y);
    }

    public void AddNoteAt(float time, int lane)
    {
        // Snap the time to the nearest subdivision (using timePerCell)
        float snappedTime = Mathf.Floor(time / timePerCell) * timePerCell;

        // Check for duplicate notes in the same lane at nearly the same time
        foreach (NoteData nd in chartNotes)
        {
            if (nd.lane == lane && Mathf.Abs(nd.time - snappedTime) < duplicateThreshold)
            {
                Debug.Log("Note already exists at lane " + lane + ", time " + snappedTime);
                return;
            }
        }

        // Create new note data and add it to the list
        NoteData newNote = new NoteData { time = snappedTime, lane = lane };
        chartNotes.Add(newNote);

        // Instantiate the note (assuming InstantiateNote(newNote) is defined)
        InstantiateNote(newNote);

        Debug.Log("Note added at time " + snappedTime + ", lane " + lane);
    }

    /// <summary>
    /// Returns the rotation for the note based on its lane.
    /// Swaps lane 0 and lane 3 if needed.
    /// </summary>
    public Quaternion GetRotationForLane(int lane)
    {
        if (lane == 0 || lane == 3)
        {
            lane = (lane == 0) ? 3 : 0;
        }
        switch (lane)
        {
            case 0: return Quaternion.Euler(0, 0, 180);
            case 1: return Quaternion.Euler(0, 0, 90);
            case 2: return Quaternion.Euler(0, 0, 270);
            case 3: return Quaternion.Euler(0, 0, 0);
            default: return Quaternion.identity;
        }
    }

    public void DeleteSelectedNotes()
    {
        foreach (GameObject go in instantiatedNotes)
        {
            Destroy(go);
        }
        instantiatedNotes.Clear();
        chartNotes.Clear();
        Debug.Log("All notes deleted.");
    }

    void ParseTimingPoints(string[] lines, ref ChartMetadata meta)
    {
        bool inTimingPoints = false;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("[TimingPoints]"))
            {
                inTimingPoints = true;
                continue;
            }
            if (inTimingPoints && !string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("["))
            {
                // Expected format: "107,362.811791"
                string[] parts = trimmed.Split(',');
                if (parts.Length >= 2)
                {
                    float offsetMs;
                    float beatLength;
                    if (float.TryParse(parts[0], out offsetMs) && float.TryParse(parts[1], out beatLength))
                    {
                        float bpmCalculated = 60000f / beatLength;
                        meta.offset = offsetMs; // You can convert to seconds if needed
                        meta.bpm = bpmCalculated;
                        meta.snap = 4; // Hard-coded per your requirement.
                        Debug.Log($"Parsed TimingPoint: Offset = {offsetMs} ms, BPM = {bpmCalculated}, Snap = 4");
                    }
                }
                break; // Stop reading timing points after the first one.
            }
        }
    }
}