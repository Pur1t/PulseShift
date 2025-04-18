using UnityEngine;
using UnityEngine.EventSystems;

public class NoteRightClickDeleter : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if right mouse button is clicked.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right click detected. Deleting note.");

            // Try to get the NoteInstance component to retrieve the corresponding NoteData.
            NoteInstance noteInst = GetComponent<NoteInstance>();
            if (noteInst != null)
            {
                if (ChartEditorManager.Instance != null)
                {
                    ChartEditorManager.Instance.RemoveNote(noteInst.noteData, gameObject);
                }
                else
                {
                    Debug.LogError("ChartEditorManager.Instance is null!");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("NoteInstance component not found on this note. Deleting note GameObject.");
                Destroy(gameObject);
            }
        }
    }
}