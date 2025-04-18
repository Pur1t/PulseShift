using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JudgementDisplay : MonoBehaviour
{
    public static JudgementDisplay Instance;

    [Header("UI Reference")]
    public Image judgementImage; // Assign the UI Image from the Canvas

    [Header("Judgement Sprites")]
    public Sprite perfectSprite;
    public Sprite greatSprite;
    public Sprite missSprite;

    [Header("Display Duration")]
    public float displayDuration = 0.5f; // How long to show the judgement (in seconds)

    private Coroutine hideCoroutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Displays the judgement sprite for a short time.
    /// </summary>
    public void ShowJudgement(JudgementType judgement)
    {
        Sprite newSprite = null;
        switch (judgement)
        {
            case JudgementType.Perfect:
                newSprite = perfectSprite;
                break;
            case JudgementType.Great:
                newSprite = greatSprite;
                break;
            case JudgementType.Miss:
                newSprite = missSprite;
                break;
        }

        if (judgementImage != null)
        {
            judgementImage.sprite = newSprite;
            judgementImage.enabled = true;

            // If a previous hide coroutine is running, stop it.
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (judgementImage != null)
            judgementImage.enabled = false;
        hideCoroutine = null;
    }
}