using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TimelineGrid : Graphic
{
    [Header("Grid Settings")]
    public float bpm = 105f;
    [Tooltip("1/4 => each beat is split into 4 lines.")]
    public float subdivisionFraction = 0.25f;
    [Tooltip("Offset in milliseconds to shift everything downward.")]
    public float offsetMs = 27f;

    [Tooltip("Song duration in seconds (useful to limit the grid height).")]
    public float songDuration = 120f;  // example

    [Tooltip("Lane count for horizontal lines.")]
    public int laneCount = 4;

    [Tooltip("Line width for main beats.")]
    public float mainBeatLineWidth = 5f;
    [Tooltip("Line width for subdivisions.")]
    public float subBeatLineWidth = 3.5f;

    [Tooltip("Color for main beat lines.")]
    public Color mainBeatColor = Color.white;
    [Tooltip("Color for subdivision lines.")]
    public Color subBeatColor = Color.gray;

    [Tooltip("Color for vertical lane lines.")]
    public Color laneLineColor = Color.white;
    [Tooltip("Width for vertical lane lines.")]
    public float laneLineWidth = 2f;

    [Tooltip("Pixels per second scale for vertical spacing (approx).")]
    public float scale = 100f; // you can refine this or do an actual rowHeight approach

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();

        // Calculate time per beat:
        float beatDuration = 60f / bpm;  // in seconds
        float subDuration = beatDuration * subdivisionFraction; // time per subdivision

        // Convert offsetMs to seconds:
        float offsetSec = offsetMs / 1000f;

        // 1) Draw horizontal lines for each subdivision up to songDuration
        // We'll go from time=0 to time=songDuration, stepping by subDuration
        // and incorporate the offset by shifting everything down by offsetSec.
        float t = 0f;
        while (t <= songDuration)
        {
            float lineTime = t + offsetSec;
            // Convert this lineTime to Y coordinate using scale
            // pivot at bottom => y= lineTime * scale
            float yPos = lineTime * scale;
            if (yPos < 0f)
            {
                // if offset makes the line negative, skip it
                t += subDuration;
                continue;
            }
            if (yPos > rect.height)
                break; // done

            // Decide if it's a main beat or a subdivision:
            // main beat if (t / beatDuration) is an integer (within a small threshold)
            float beatIndex = t / beatDuration;
            bool isMainBeat = (Mathf.Abs(beatIndex - Mathf.Round(beatIndex)) < 0.001f);

            float lineW = isMainBeat ? mainBeatLineWidth : subBeatLineWidth;
            Color lineC = isMainBeat ? mainBeatColor : subBeatColor;

            AddLine(vh,
                    new Vector2(rect.xMin, yPos),
                    new Vector2(rect.xMax, yPos),
                    lineW,
                    lineC);

            t += subDuration;
        }

        // 2) Draw vertical lines for lanes:
        float cellWidth = rect.width / laneCount;
        for (int lane = 0; lane <= laneCount; lane++)
        {
            float xPos = rect.xMin + lane * cellWidth;
            AddLine(vh,
                    new Vector2(xPos, rect.yMin),
                    new Vector2(xPos, rect.yMax),
                    laneLineWidth,
                    laneLineColor);
        }
    }

    void AddLine(VertexHelper vh, Vector2 start, Vector2 end, float width, Color color)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perp = new Vector2(-direction.y, direction.x);
        Vector2 offset = perp * (width / 2f);

        int index = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = start - offset;
        vh.AddVert(v);
        v.position = start + offset;
        vh.AddVert(v);
        v.position = end + offset;
        vh.AddVert(v);
        v.position = end - offset;
        vh.AddVert(v);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}