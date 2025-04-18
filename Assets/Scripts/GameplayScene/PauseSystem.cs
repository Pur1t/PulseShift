using UnityEngine;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI pausePrompt;   // "Game Paused" prompt
    private GameObject pausePromptParent;

    [Header("Pause Settings")]
    public float requiredPHoldTime = 1f;  // Hold "P" for 1 second to toggle pause

    [HideInInspector]
    public bool gameStarted = false;
    [HideInInspector]
    public bool isPaused = false;

    private float pHoldTimer = 0f;

    private void Awake()
    {
        pausePromptParent = pausePrompt.transform.parent.gameObject;
        pausePromptParent.SetActive(false);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Pre-start: Show "Press Enter to start" prompt and wait for Enter.
        if (!gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                gameStarted = true;
                // Ensure game is unpaused
                UnpauseGame();
                // Also tell Conductor to start playing the song.
                Conductor.Instance.PlaySong();
            }
            return;
        }

        // During gameplay, check for P key hold to toggle pause.
        if (Input.GetKey(KeyCode.P))
        {
            pHoldTimer += Time.unscaledDeltaTime; // use unscaledDeltaTime so that pause state doesn't affect timer
            if (pHoldTimer >= requiredPHoldTime)
            {
                if (!isPaused)
                    PauseGame();
                else
                    UnpauseGame();
                pHoldTimer = 0f;
            }
        }
        else
        {
            pHoldTimer = 0f;
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        if (pausePrompt != null)
        {
            pausePromptParent.SetActive(true);
            pausePrompt.gameObject.SetActive(true);
        }
        Time.timeScale = 0f; // Freeze time for Update methods that use deltaTime.
        Conductor.Instance.PauseSong();
    }

    public void UnpauseGame()
    {
        if (!isPaused) return;
        isPaused = false;
        if (pausePrompt != null)
        {
            pausePromptParent.SetActive(false);
            pausePrompt.gameObject.SetActive(false);
        }
        Time.timeScale = 1f; // Resume normal time
        Conductor.Instance.UnpauseSong();
    }
}
