using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SliderController : MonoBehaviour
{
    [Header("UI References")]
    public Slider timeSlider;                    // Slider controlling the timeline
    public TextMeshProUGUI currentTimestamp;     // Current time text
    public TextMeshProUGUI remainTimestamp;      // Remaining time text
    public ScrollRect timelineScrollRect;        // The ScrollRect that contains the grid panel

    private AudioSource musicSource;             // Obtained from EditorConductor
    private float songDuration = 300f;           // Default duration; will update from audio clip if available
    private bool isDragging = false;

    void Start()
    {
        StartCoroutine(InitializeSlider());
    }

    IEnumerator InitializeSlider()
    {
        // Wait until EditorConductor and its audio clip are available.
        while (EditorConductor.Instance == null ||
               EditorConductor.Instance.musicSource == null ||
               EditorConductor.Instance.musicSource.clip == null)
        {
            yield return null;
        }
        musicSource = EditorConductor.Instance.musicSource;
        songDuration = musicSource.clip.length;
        if (timeSlider != null)
        {
            timeSlider.minValue = 0;
            timeSlider.maxValue = songDuration;
            timeSlider.value = 0;
            timeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        // Optionally pause audio initially.
        musicSource.time = 0;
        musicSource.Pause();
        Debug.Log("Slider initialized. Song duration: " + songDuration + " seconds.");
    }

    void Update()
    {
        if (musicSource == null || musicSource.clip == null)
            return;

        // If not dragging, update slider value from audio time.
        if (!isDragging && musicSource.isPlaying)
        {
            timeSlider.SetValueWithoutNotify(musicSource.time);
        }

        float currentTime = timeSlider.value;
        currentTimestamp.text = FormatTime(currentTime);
        float remaining = Mathf.Max(0, songDuration - currentTime);
        remainTimestamp.text = "-" + FormatTime(remaining);

        // Update vertical scroll of the ScrollRect.
        // Assuming grid panel (Content) height corresponds to the song timeline,
        // and verticalNormalizedPosition is 0 at bottom and 1 at top.
        if (timelineScrollRect != null)
        {
            // We want time=0 at the bottom, so normalized position = currentTime / songDuration.
            timelineScrollRect.verticalNormalizedPosition = currentTime / songDuration;
        }
    }

    public void OnSliderChanged(float value)
    {
        if (isDragging && musicSource != null)
        {
            musicSource.time = value;
            Debug.Log("Slider changed (dragging): audio time set to " + FormatTime(value));
        }
    }

    public void OnSliderBeginDrag()
    {
        isDragging = true;
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("Slider drag started: audio paused.");
        }
    }

    public void OnSliderEndDrag()
    {
        isDragging = false;
        if (musicSource != null)
        {
            musicSource.time = timeSlider.value;
            Debug.Log("Slider drag ended: audio time set to " + FormatTime(timeSlider.value));
        }
    }

    string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int sec = (int)(seconds % 60f);
        int millis = (int)((seconds - Mathf.Floor(seconds)) * 1000);
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, sec, millis);
    }
}