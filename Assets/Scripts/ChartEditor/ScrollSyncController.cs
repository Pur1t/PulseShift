using UnityEngine;
using UnityEngine.UI;

public class ScrollSyncController : MonoBehaviour
{
    [Tooltip("The slider controlling the timeline (0 to songDuration)")]
    public Slider timeSlider;
    [Tooltip("The ScrollRect that contains the grid panel (Content)")]
    public ScrollRect gridScrollRect;
    [Tooltip("Total duration of the song in seconds (should match slider max)")]
    public float songDuration = 300f;

    public void Start()
    {
        if (timeSlider == null || gridScrollRect == null)
        {
            Debug.LogError("Please assign both the timeSlider and gridScrollRect in the Inspector.");
            return;
        }
        // Optionally update songDuration from the slider's max value:
        songDuration = timeSlider.maxValue;
        timeSlider.onValueChanged.AddListener(UpdateScrollRect);
    }

    public void UpdateScrollRect(float currentTime)
    {
        float pitch = 1f;
        if (EditorConductor.Instance != null && EditorConductor.Instance.musicSource != null)
            pitch = EditorConductor.Instance.musicSource.pitch;

        // Effective song duration decreases if the pitch is increased.
        float effectiveSongDuration = songDuration / pitch;
        // Invert so that time=0 is at the top.
        float normalizedPos = 1f - (currentTime / songDuration);
        gridScrollRect.verticalNormalizedPosition = normalizedPos;
        Debug.Log("Slider value: " + currentTime + " sec => normalized: " + normalizedPos);
    }
}