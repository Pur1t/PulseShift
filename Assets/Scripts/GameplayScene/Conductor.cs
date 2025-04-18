using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;

public class Conductor : MonoBehaviour
{
    public static Conductor Instance;
    public AudioSource musicSource;
    public float bpm = 120f;
    public float offset = 0f;

    private float secondsPerBeat;
    private float songStartTime;
    private float pauseStartTime = 0f; // when pause begins

    public bool hasStartedPlaying = false;

    public float songPosition;
    public float songPositionInBeats;

    public int chartHighScore = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        secondsPerBeat = 60f / bpm;
        if (!string.IsNullOrEmpty(SelectedSongData.audioFilePath))
        {
            Debug.Log("Start Coroutine (Conductor.cs)");
            StartCoroutine(LoadAudioClip("file://" + SelectedSongData.audioFilePath));
        }
        else
        {
            Debug.LogError("Audio file path not set in SelectedSongData.");
        }
    }

    IEnumerator LoadAudioClip(string url)
    {
        Debug.Log("Loading audio from URL: " + url);
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError("Error loading audio: " + uwr.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
            if (clip != null)
            {
                musicSource.clip = clip;
                // Do not start song automatically; wait for "Press Enter" (PauseManager handles that).
            }
            else
            {
                Debug.LogError("Loaded clip is null!");
            }
        }
    }

    public void PlaySong()
    {
        if (musicSource.clip != null)
        {
            songStartTime = (float)AudioSettings.dspTime;
            musicSource.Play();
        }
        else
        {
            Debug.LogError("No audio clip loaded in Conductor.");
        }
    }

    public void PauseSong()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            pauseStartTime = (float)AudioSettings.dspTime;
        }
    }

    public void UnpauseSong()
    {
        float pauseDuration = (float)AudioSettings.dspTime - pauseStartTime;
        // Adjust songStartTime so that songPosition doesn't jump.
        songStartTime += pauseDuration;
        musicSource.UnPause();
    }

    public void SaveScore(int score, float accuracy) 
    {
        string formatAccuracy = accuracy.ToString("F2");
        // == 100.00 SSS, >= 99.00 SS, >=97.00 S, >= 95.00 AA, >=93.00 A, >=90.00 B, >= 80.00 C, >= 70.00 D, <70.00 F
        string rank = "X";

        if (accuracy == 100.00) { rank = "SSS"; }
        else if (accuracy >= 99.00) { rank = "SS"; }
        else if (accuracy >= 97.00) { rank = "S"; }
        else if (accuracy >= 95.00) { rank = "AA"; }
        else if (accuracy >= 93.00) { rank = "A"; }
        else if (accuracy >= 90.00) { rank = "B"; }
        else if (accuracy >= 80.00) { rank = "C"; }
        else if (accuracy >= 70.00) { rank = "D"; }
        else { rank = "F"; }

        string chartsFolder = Path.Combine(Application.streamingAssetsPath, "Charts");
        string scoreContent = "[Score]\nScore: " + Mathf.FloorToInt(score) + "\n" + "Accuracy: " + formatAccuracy + "\n" + "Rank: " + rank;
        string scorePath = Path.Combine(chartsFolder, "score.txt");
        File.WriteAllText(scorePath, scoreContent);

        if (!string.IsNullOrEmpty(SelectedSongData.audioFilePath))
        {
            string audioDirectory = Path.GetDirectoryName(SelectedSongData.audioFilePath);
            string scoreAudioPath = Path.Combine(audioDirectory, "score.txt");
            File.WriteAllText(scoreAudioPath, scoreContent);
            Debug.Log("Score also saved in: " + scoreAudioPath);
        }
        else
        {
            Debug.LogError("Audio file path is not set. Could not save score in audio directory.");
        }
    }

    void FixedUpdate()
    {
        // Do not update songPosition if the game is paused.
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            return;

        if (musicSource.clip != null)
        {
            songPosition = (float)(AudioSettings.dspTime - songStartTime) - offset;
            songPositionInBeats = songPosition / secondsPerBeat;
        }

        //Debug.Log("(Conductor) musicSource.time = " + musicSource.time);
        if (hasStartedPlaying && !musicSource.isPlaying && musicSource.time < 0.01f) // the song is end
        {
            Debug.Log("Song Ended...");
            hasStartedPlaying = false;

            // Save Score (if high score is achieved)
            if (ScoreSystem.Instance.score >= SelectedSongData.chartHighScore)
            {
                Debug.Log("New High Score!");
                SaveScore(ScoreSystem.Instance.score, AccuracySystem.Instance.currentAccuracy);
            }
            else
            {
                Debug.Log("no new High Score.... (booooo)");
            }

            // Return to music selected scene
            SceneManager.LoadScene("MusicSelectScene");
        }
    }
}
