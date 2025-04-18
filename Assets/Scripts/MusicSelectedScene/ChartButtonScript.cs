using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChartButtonScript : MonoBehaviour
{
    // Static reference to track the currently selected button.
    private static ChartButtonScript currentlySelectedButton = null;

    // UI references to update when showing score details.
    [Header("UI Elements (Set these in the Inspector)")]
    public TextMeshProUGUI scoreText;    // e.g., displays "Score: 123,456"
    public TextMeshProUGUI accuracyText; // e.g., displays "Accuracy: 100.00"
    public TextMeshProUGUI rankText;     // e.g., displays "Rank: SSS"
    private TextMeshProUGUI rank;

    // File paths specific to this song.
    [Header("Song File Paths (Set these for each button)")]
    public string scoreFilePath;  // e.g., .../score.txt
    public string chartFilePath;  // e.g., .../chart.txt
    public string audioFilePath;  // e.g., .../song.mp3


    public void Start()
    {
        GameObject tmpGO = GameObject.Find("ScoreShowTMP");
        scoreText = tmpGO.GetComponent<TextMeshProUGUI>();

        tmpGO = GameObject.Find("AccuracyShowTMP");
        accuracyText = tmpGO.GetComponent<TextMeshProUGUI>();

        tmpGO = GameObject.Find("RankShowTMP");
        rankText = tmpGO.GetComponent<TextMeshProUGUI>();

        tmpGO = GameObject.Find("RankTMP");
        rank = tmpGO.GetComponent<TextMeshProUGUI>();
        rank.enabled = false;
    }

    // This function should be linked to the Button's OnClick event.
    public void OnSongButtonClicked()
    {
        // If a different button is currently selected, clear its selection.
        if (currentlySelectedButton != null && currentlySelectedButton != this)
        {
            currentlySelectedButton.ResetSelection();
        }

        if (currentlySelectedButton != this)
        {
            // First click for this button: update UI and mark this button as selected.
            currentlySelectedButton = this;
            UpdateScoreDisplay();
        }
        else
        {

            // Second click for this button (without interruption): load the gameplay scene.
            SelectedSongData.chartFilePath = chartFilePath;
            SelectedSongData.audioFilePath = audioFilePath;

            Debug.Log(SelectedSongData.chartFilePath);
            Debug.Log(SelectedSongData.audioFilePath);

            // Get the currently active scene's name.
            string currentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log("Current scene: " + currentSceneName);

            // Check if the scene is MusicSelectedEditScene or MusicSelectedScene.
            if (currentSceneName == "MusicSelectedEditScene")
            {
                // User is in the Chart Editor selection scene.
                // Load the chart editor page.
                SceneManager.LoadScene("ChartEditorScene"); // Replace with your actual scene name for editing.
            }
            else if (currentSceneName == "MusicSelectScene")
            {
                // User is in the normal Music Selected scene.
                // Load the gameplay scene.
                SceneManager.LoadScene("GameplayScene"); // Replace with your actual gameplay scene name.
            }
            else
            {
                Debug.LogWarning("Unexpected scene: " + currentSceneName);
            }
        }
    }

    /// <summary>
    /// Resets the selection state.
    /// </summary>
    public void ResetSelection()
    {
        if (currentlySelectedButton == this)
        {
            currentlySelectedButton = null;
        }
    }

    /// <summary>
    /// Reads the score file and updates the UI texts.
    /// Expected file format:
    /// 
    /// [Score]
    /// Score: 123456
    /// Accuracy: 100.00
    /// Rank: SSS
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (!File.Exists(scoreFilePath))
        {
            Debug.LogError("Score file not found: " + scoreFilePath);
            return;
        }

        // Read the score file lines.
        string[] lines = File.ReadAllLines(scoreFilePath);
        bool inScoreSection = false;
        Dictionary<string, string> scoreDict = new Dictionary<string, string>();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine == "[Score]")
            {
                inScoreSection = true;
                continue;
            }
            else if (trimmedLine.StartsWith("[") && inScoreSection)
            {
                // End of the [Score] section.
                break;
            }
            if (inScoreSection)
            {
                int colonIndex = trimmedLine.IndexOf(':');
                if (colonIndex > 0)
                {
                    string key = trimmedLine.Substring(0, colonIndex).Trim();
                    string value = trimmedLine.Substring(colonIndex + 1).Trim();
                    scoreDict[key] = value;
                }
            }
        }

        // Make sure the required keys exist.
        if (scoreDict.ContainsKey("Score") && scoreDict.ContainsKey("Accuracy") && scoreDict.ContainsKey("Rank"))
        {
            if (int.TryParse(scoreDict["Score"], out int scoreValue) &&
                float.TryParse(scoreDict["Accuracy"], out float accuracyValue))
            {
                // update chartHighScore
                SelectedSongData.chartHighScore = scoreValue;
                // Format values as desired.
                string formattedScore = scoreValue.ToString("N0");   // e.g., 123,456
                string formattedAccuracy = accuracyValue.ToString("F2"); // e.g., 100.00
                string rankValue = scoreDict["Rank"];

                // Update the UI text components.
                rank.enabled = true;
                scoreText.text = formattedScore;
                accuracyText.text = "(" + formattedAccuracy + ")";
                if (rankValue == "SSS" || rankValue == "SS" || rankValue == "S") accuracyText.color = new Color32(0xE0, 0xCD, 0x32, 0xFF);
                else if (rankValue == "AA" || rankValue == "A") accuracyText.color = new Color32(0xF0, 0x0F, 0x30, 0xFF);
                else if (rankValue == "B") accuracyText.color = new Color32(0x0F, 0x7E, 0xF0, 0xFF);
                else accuracyText.color = new Color32(0x9F, 0x97, 0x96, 0xFF);

                rankText.text = rankValue;
                if (rankValue == "SSS" || rankValue == "SS" || rankValue == "S") rankText.color = new Color32(0xE0, 0xCD, 0x32, 0xFF);
                else if (rankValue == "AA" || rankValue == "A") rankText.color = new Color32(0xF0, 0x0F, 0x30, 0xFF);
                else if (rankValue == "B") rankText.color = new Color32(0x0F, 0x7E, 0xF0, 0xFF);
                else rankText.color = new Color32(0x9F, 0x97, 0x96, 0xFF);
            }
            else
            {
                Debug.LogError("Error parsing score or accuracy values.");
            }
        }
        else
        {
            Debug.LogError("Score file missing one or more required keys (Score, Accuracy, Rank).");
        }
    }
}