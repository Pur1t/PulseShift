using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;  // Required for TextMeshPro

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private GameObject myObject;   // Prefab to instantiate
    [SerializeField] private Transform parentObject;  // Parent for spawned objects
    [SerializeField] private string chartsFolderPath = "Charts"; // Relative folder path inside Assets

    void Start()
    {
        // Build the absolute path to the Charts folder.
        string fullPath = Path.Combine(Application.streamingAssetsPath, chartsFolderPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError("Charts folder not found at: " + fullPath);
            return;
        }

        // Get all song subfolders
        string[] songFolders = Directory.GetDirectories(fullPath);

        foreach (string songFolder in songFolders)
        {
            // Define the expected file paths for each song folder.
            string scoreFile = Path.Combine(songFolder, "score.txt");
            string chartFile = Path.Combine(songFolder, "chart.txt");

            // Supported audio extensions
            string[] audioExtensions = { "*.mp3", "*.ogg", "*.wav", "*.flac", "*.m4a" };

            List<string> audioFiles = new List<string>();

            // Search for all supported audio files in the folder
            foreach (string extension in audioExtensions)
            {
                audioFiles.AddRange(Directory.GetFiles(songFolder, extension));
            }

            string audioFile = audioFiles[0];

            // Look for a .txt file in the song folder
            string[] txtFiles = Directory.GetFiles(songFolder, "*.txt");
            if (txtFiles.Length > 0)
            {
                // Read the metadata section from the first .txt file found
                string metadataSection = GetMetadataFromFile(txtFiles[0]);
                // Parse the metadata into a key-value dictionary
                Dictionary<string, string> metadataDict = ParseMetadata(metadataSection);
                //foreach (var key in metadataDict.Keys)
                //{
                //    Debug.Log($"Key: {key}, Value: {metadataDict[key]}");
                //}

                // Instantiate your prefab
                GameObject temp = Instantiate(myObject, parentObject);

                // Get the ChartButtonScript component from the instantiated button.
                ChartButtonScript buttonScript = temp.GetComponent<ChartButtonScript>();
                if (buttonScript != null)
                {
                    // Assign the file paths to the ChartButtonScript.
                    buttonScript.scoreFilePath = scoreFile;
                    buttonScript.chartFilePath = chartFile;
                    buttonScript.audioFilePath = audioFile;
                }
                else
                {
                    Debug.LogError("ChartButtonScript not found on the instantiated object: " + temp.name);
                }

                // Get the TMP_Text component from the instantiated prefab (or its children)
                TMP_Text tmpText = temp.GetComponentInChildren<TMP_Text>();
                if (tmpText != null)
                {
                    // Retrieve "Artist" and "Song" values, using defaults if missing.
                    string artist = metadataDict.ContainsKey("Artist") ? metadataDict["Artist"] : "Unknown Artist";
                    string song = metadataDict.ContainsKey("Song") ? metadataDict["Song"] : "Unknown Song";

                    // Set the TextMeshPro text to the format "Artist - Song"
                    tmpText.text = artist + " - " + song;
                }
                else
                {
                    Debug.LogWarning("TMP_Text component not found in instantiated prefab.");
                }
            }
            else
            {
                Debug.LogWarning("No .txt file found in folder: " + songFolder);
            }
        }
    }

    /// <summary>
    /// Reads the .txt file and returns only the [Metadata] section as a single string.
    /// </summary>
    private string GetMetadataFromFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        bool inMetadata = false;
        List<string> metadataLines = new List<string>();

        foreach (string line in lines)
        {
            if (line.Trim() == "[Metadata]")
            {
                inMetadata = true;
                continue; // Skip header line.
            }
            else if (line.StartsWith("[") && inMetadata)
            {
                // Reached a new section
                break;
            }

            if (inMetadata)
            {
                metadataLines.Add(line);
            }
        }

        return string.Join("\n", metadataLines);
    }

    /// <summary>
    /// Parses metadata in the format "Key: Value" from a string.
    /// </summary>
    private Dictionary<string, string> ParseMetadata(string metadataText)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string[] lines = metadataText.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                string key = line.Substring(0, colonIndex).Trim();
                string value = line.Substring(colonIndex + 1).Trim();
                dict[key] = value;
            }
        }
        return dict;
    }
}