using UnityEngine;
using UnityEngine.UI;

public class ParallaxMultipleBackgrounds : MonoBehaviour
{
    [SerializeField] float moveSpeed = 100f;  // Scrolling speed
    [SerializeField] bool scrollLeft = true;  // True for leftward scrolling, false for rightward
    [SerializeField] Image[] backgrounds;     // Drag your background UI Images (left, middle, right, etc.) into this array

    float backgroundWidth;

    private void Start()
    {
        // Ensure moveSpeed is correctly signed.
        moveSpeed = scrollLeft ? -Mathf.Abs(moveSpeed) : Mathf.Abs(moveSpeed);

        if (backgrounds.Length > 0)
        {
            // Get the width from the RectTransform (UI coordinates).
            backgroundWidth = backgrounds[0].GetComponent<RectTransform>().rect.width;
        }

        // (Optional) You can sort the backgrounds array here by their localPosition.x if needed.
    }

    private void Update()
    {
        float delta = moveSpeed * Time.deltaTime;

        // Move all backgrounds.
        foreach (var bg in backgrounds)
        {
            bg.transform.localPosition += new Vector3(delta, 0f, 0f);
        }

        // Check each background for off-screen condition and reposition it accordingly.
        if (scrollLeft)
        {
            foreach (var bg in backgrounds)
            {
                if (bg.transform.localPosition.x <= -backgroundWidth)
                {
                    // When scrolling left, reposition offscreen backgrounds to the right of the current rightmost background.
                    float rightmostX = GetRightmostX();
                    bg.transform.localPosition = new Vector3(rightmostX + backgroundWidth, bg.transform.localPosition.y, bg.transform.localPosition.z);
                }
            }
        }
        else
        {
            foreach (var bg in backgrounds)
            {
                if (bg.transform.localPosition.x >= backgroundWidth)
                {
                    // When scrolling right, reposition offscreen backgrounds to the left of the current leftmost background.
                    float leftmostX = GetLeftmostX();
                    bg.transform.localPosition = new Vector3(leftmostX - backgroundWidth, bg.transform.localPosition.y, bg.transform.localPosition.z);
                }
            }
        }
    }

    // Finds the rightmost x-position among all backgrounds.
    float GetRightmostX()
    {
        float rightmost = float.MinValue;
        foreach (var bg in backgrounds)
        {
            if (bg.transform.localPosition.x > rightmost)
            {
                rightmost = bg.transform.localPosition.x;
            }
        }
        return rightmost;
    }

    // Finds the leftmost x-position among all backgrounds.
    float GetLeftmostX()
    {
        float leftmost = float.MaxValue;
        foreach (var bg in backgrounds)
        {
            if (bg.transform.localPosition.x < leftmost)
            {
                leftmost = bg.transform.localPosition.x;
            }
        }
        return leftmost;
    }
}