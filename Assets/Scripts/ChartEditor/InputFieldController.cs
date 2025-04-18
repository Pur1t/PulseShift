using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldController : MonoBehaviour
{
    // Use RectTransform instead of Canvas for the metadata window.
    public RectTransform canvasRef;
    public Button confirmButton;

    public TMP_InputField artistNameTMP;
    public TMP_InputField songTitleTMP;
    public TMP_InputField bpmTMP;
    public TMP_InputField offsetTMP;

    void Start()
    {
        if (canvasRef != null)
        {
            // Disable the canvas GameObject by setting it inactive.
            canvasRef.gameObject.SetActive(false);

            // Check if metadata exists and then assign values to input fields.
            if (EditorConductor.Instance != null && EditorConductor.Instance.metadata != null)
            {
                artistNameTMP.text = EditorConductor.Instance.metadata.artist;
                songTitleTMP.text = EditorConductor.Instance.metadata.songTitle;
                bpmTMP.text = EditorConductor.Instance.metadata.bpm.ToString();
                offsetTMP.text = EditorConductor.Instance.metadata.offset.ToString();
            }
        }
    }

    public void SaveMetadata() // Called when confirmButton is clicked
    {
        // Retrieve text from the TMP_InputFields.
        string artistInput = artistNameTMP.text;
        string songTitleInput = songTitleTMP.text;
        string bpmInput = bpmTMP.text;
        string offsetInput = offsetTMP.text;

        // Parse BPM (float) and offset (int) from their respective input texts.
        float bpm = 120f;  // default value in case parsing fails
        if (!float.TryParse(bpmInput, out bpm))
        {
            Debug.LogWarning("Could not parse BPM input, using default value: " + bpm);
        }

        int offset = 0;  // default value in case parsing fails
        if (!int.TryParse(offsetInput, out offset))
        {
            Debug.LogWarning("Could not parse offset input, using default value: " + offset);
        }

        // Save the metadata
        EditorConductor.Instance.metadata.artist = artistInput;
        EditorConductor.Instance.metadata.songTitle = songTitleInput;
        EditorConductor.Instance.metadata.bpm = bpm;
        EditorConductor.Instance.metadata.offset = offset;

        Debug.Log("Metadata saved: " + EditorConductor.Instance.metadata.artist + ", " + EditorConductor.Instance.metadata.songTitle + ", " + EditorConductor.Instance.metadata.bpm + ", " + EditorConductor.Instance.metadata.offset);
    }

    public void OpenMetadataWindow()
    {
        canvasRef.gameObject.SetActive(true);
    }

    public void CloseMetadataWindow()
    {
        canvasRef.gameObject.SetActive(false);
    }
}