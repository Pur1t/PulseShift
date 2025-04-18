using UnityEngine;
using UnityEngine.EventSystems;

public class GridClickHandler : MonoBehaviour, IPointerClickHandler
{
    // Reference to your ChartEditorManager.
    public ChartEditorManager editorManager;

    private void Awake()
    {
        if (editorManager == null)
            editorManager = ChartEditorManager.Instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Debug.LogError("Failed to convert screen point to local point.");
            return;
        }

        // Horizontal calculation:
        // Convert localPoint.x (range: [-width/2, width/2]) to [0, width]
        float panelWidth = rt.rect.width;
        float adjustedX = localPoint.x + (panelWidth * 0.5f);
        int lane = Mathf.Clamp(Mathf.FloorToInt(adjustedX / (panelWidth / editorManager.laneCount)), 0, editorManager.laneCount - 1);

        // Vertical calculation:
        // With the grid panel's pivot set to (0.5, 0), localPoint.y is measured from the bottom.
        float effectiveY = localPoint.y - editorManager.verticalOffset;
        if (effectiveY < 0)
            effectiveY = 0;
        // Compute fractional row index (each row represents timePerCell seconds)
        float rowIndex = effectiveY / editorManager.rowHeight;
        // Calculate note time using the fixed time per cell
        float noteTime = rowIndex * editorManager.timePerCell;
        // Snap noteTime to the nearest subdivision (floor it)
        noteTime = Mathf.Floor(noteTime / editorManager.timePerCell) * editorManager.timePerCell;

        editorManager.AddNoteAt(noteTime, lane);
        Debug.Log("Grid clicked: localPoint=" + localPoint + ", lane=" + lane + ", noteTime=" + noteTime);
    }
}