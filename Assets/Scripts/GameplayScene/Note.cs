using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Note : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [Header("Note Settings")]
    public int lane;                 // Lane index (0 to 3)
    public float targetTime;         // The time when the note should reach the receptor (in seconds)
    public Transform targetPosition; // The receptor's transform for this note
    public float travelTime = 1.5f;    // How long the note takes to travel

    // Internal variables
    private Vector3 startPosition;   // The fixed spawn position (recorded at instantiation)
    private float spawnTime;         // The time when the note was spawned

    // NEW: For chart editor functionality:
    private Vector3 originalPosition; // To record position at drag start
    public bool selected = false;       // Whether this note is selected
    public Image imageComponent;        // For visual feedback (e.g. color change)

    void Start()
    {
        startPosition = transform.position;
        spawnTime = Conductor.Instance ? Conductor.Instance.songPosition : Time.time;
        originalPosition = startPosition;
        imageComponent = GetComponent<Image>(); // Assumes your note prefab has an Image component
        Debug.Log("Note spawned in lane " + lane + " at position: " + startPosition);
    }

    void Update()
    {
        float currentTime = Conductor.Instance ? Conductor.Instance.songPosition : Time.time;
        float progress = (currentTime - spawnTime) / travelTime;
        // Move note from startPosition to receptor (targetPosition) even if progress > 1.
        transform.position = Vector3.LerpUnclamped(startPosition, targetPosition.position, progress);
    }

    // Called when dragging begins.
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;
    }

    // Called during dragging.
    public void OnDrag(PointerEventData eventData)
    {
        // Move the note; if using UI, adjust according to canvas scale.
        transform.position += (Vector3)eventData.delta;
    }
}
