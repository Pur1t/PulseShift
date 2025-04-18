using UnityEngine;
using UnityEngine.EventSystems;

public class NoteEditor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public NoteData noteData; // Reference to this note's data.
    private RectTransform rt;
    private Vector2 originalPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rt.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Get grid panel and lane count from the manager.
        RectTransform gridPanel = ChartEditorManager.Instance.gridPanel;
        int laneCount = ChartEditorManager.Instance.laneCount;
        float panelWidth = gridPanel.rect.width;
        float cellWidth = panelWidth / laneCount;

        // Reverse the horizontal offset (because GetNotePosition subtracts it).
        float adjustedX = rt.anchoredPosition.x + ChartEditorManager.Instance.horizontalOffset;
        int newLane = Mathf.Clamp(Mathf.FloorToInt(adjustedX / cellWidth), 0, laneCount - 1);

        // Calculate new time:
        // Remove the vertical offset, then divide by rowHeight (each row represents a time interval)
        float newTime = (rt.anchoredPosition.y - ChartEditorManager.Instance.verticalOffset) / ChartEditorManager.Instance.rowHeight;

        // Update note data.
        noteData.lane = newLane;
        noteData.time = newTime;

        // Snap the note's position to the center of the grid cell.
        rt.anchoredPosition = ChartEditorManager.Instance.GetNotePosition(newTime, newLane);
        rt.rotation = ChartEditorManager.Instance.GetRotationForLane(newLane);

        Debug.Log("Note repositioned to lane " + newLane + ", time " + newTime);
    }
}
