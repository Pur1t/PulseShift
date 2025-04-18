using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class EditorConductor : MonoBehaviour
{
    public static EditorConductor Instance;

    [Header("Audio Settings")]
    public AudioSource musicSource; // Assign via Inspector

    [Header("Chart Metadata")]
    public ChartMetadata metadata;  // Loaded from chart.txt's [Metadata] section.

    public float songPosition => musicSource != null ? musicSource.time : 0f;
    public float songLength => (musicSource != null && musicSource.clip != null) ? musicSource.clip.length : 0f;

    private bool audioInitialized = false; // Ensure we set time to 0 only once

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    void Start()
    {
        if (string.IsNullOrEmpty(SelectedSongData.audioFilePath) || string.IsNullOrEmpty(SelectedSongData.chartFilePath))
        {
            Debug.LogError("SelectedSongData paths are not set!");
            return;
        }
        StartCoroutine(LoadAudioClip());
        StartCoroutine(LoadChartMetadata());
    }

    IEnumerator LoadAudioClip()
    {
        string url = "file://" + SelectedSongData.audioFilePath;
        Debug.Log("Loading audio from URL: " + url);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("Error loading audio: " + request.error);
                yield break;
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip == null)
                {
                    Debug.LogError("DownloadHandler returned a null clip.");
                    yield break;
                }
                if (musicSource != null)
                {
                    musicSource.clip = clip;
                    // Log the clip's name to check if it's assigned correctly.
                    Debug.Log("Audio clip assigned: " + musicSource.clip.name);
                    if (!audioInitialized)
                    {
                        musicSource.time = 0;
                        audioInitialized = true;
                    }
                    Debug.Log("Audio loaded. Duration: " + clip.length + " seconds.");
                }
            }
        }
    }

    IEnumerator LoadChartMetadata()
    {
        if (!File.Exists(SelectedSongData.chartFilePath))
        {
            Debug.LogError("Chart file not found: " + SelectedSongData.chartFilePath);
            yield break;
        }

        string[] lines = File.ReadAllLines(SelectedSongData.chartFilePath);
        ChartMetadata meta = new ChartMetadata();
        bool inMetadata = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("[Metadata]"))
            {
                inMetadata = true;
                continue;
            }
            if (inMetadata && trimmed.StartsWith("["))
            {
                // End metadata section.
                break;
            }
            if (inMetadata && trimmed.Contains(":"))
            {
                int colonIndex = trimmed.IndexOf(':');
                string key = trimmed.Substring(0, colonIndex).Trim();
                string value = trimmed.Substring(colonIndex + 1).Trim();
                if (key.Equals("Artist", System.StringComparison.OrdinalIgnoreCase))
                {
                    meta.artist = value;
                }
                else if (key.Equals("Title", System.StringComparison.OrdinalIgnoreCase) ||
                         key.Equals("Song", System.StringComparison.OrdinalIgnoreCase))
                {
                    meta.songTitle = value;
                }
                // Optionally, process BPM/Offset/Snap here if present.
            }
        }

        // Now process the timing points
        ParseTimingPoints(lines, ref meta);

        // Now assign to your manager's metadata.
        metadata = meta;
        Debug.Log("Chart metadata loaded: Title = " + meta.songTitle +
                  ", Artist = " + meta.artist +
                  ", BPM = " + meta.bpm +
                  ", Offset = " + meta.offset +
                  ", Snap = " + meta.snap);

        yield break;
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

            if (inTimingPoints && !string.IsNullOrEmpty(trimmed))
            {
                // We expect a line like: "107,362.811791"
                string[] parts = trimmed.Split(',');
                if (parts.Length >= 2)
                {
                    float offsetMs;
                    float beatLength;
                    if (float.TryParse(parts[0], out offsetMs) && float.TryParse(parts[1], out beatLength))
                    {
                        float bpmCalculated = 60000f / beatLength;
                        meta.offset = offsetMs;   // You might convert to seconds later if needed.
                        meta.bpm = bpmCalculated;
                        meta.snap = 4;  // Hard-coded as requested.
                        Debug.Log($"Parsed TimingPoint: Offset = {offsetMs} ms, BPM = {bpmCalculated}, Snap = 4");
                    }
                }
                break; // only process the first timing point
            }
        }
    }

    public void PlaySong()
    {
        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.Play();
            Debug.Log("EditorConductor: Audio playing.");
        }
    }

    public void PauseSong()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("EditorConductor: Audio paused.");
        }
    }

    public void TogglePlayPause()
    {
        if (musicSource != null)
        {
            if (musicSource.isPlaying)
                PauseSong();
            else
                PlaySong();
        }
    }

    void Update()
    {
        // Increase speed with '=' key
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            // Increase pitch by 0.05, maximum 2.0 (or any desired upper bound)
            musicSource.pitch = Mathf.Clamp(musicSource.pitch + 0.05f, 0.25f, 2.0f);
            Debug.Log("Audio pitch increased: " + musicSource.pitch);
        }
        // Decrease speed with '-' key
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            // Decrease pitch by 0.05, minimum 0.25
            musicSource.pitch = Mathf.Clamp(musicSource.pitch - 0.05f, 0.25f, 2.0f);
            Debug.Log("Audio pitch decreased: " + musicSource.pitch);
        }
    }
}