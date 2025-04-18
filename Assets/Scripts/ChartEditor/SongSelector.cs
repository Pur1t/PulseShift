using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using System.Diagnostics;
using System.IO;

public class SongSelector : MonoBehaviour, IPointerClickHandler
{
    // Reference to the UI element that acts as the drop or click area.
    public GameObject clickArea;

    private void Awake()
    {
        if (clickArea == null)
        {
            GameObject tmp = GameObject.Find("DropBackground");
            if (tmp != null)
                clickArea = tmp;
            else
                UnityEngine.Debug.LogError("DropBackground not found!");
        }
    }

    // IPointerClickHandler implementation: called when the UI element is clicked.
    public void OnPointerClick(PointerEventData eventData)
    {
        OpenSongSelector();
    }

    // Opens a file dialog so the user can select an audio file.
    public void OpenSongSelector()
    {
        // Set filters to show supported audio files.
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Audio Files", ".mp3", ".wav", ".ogg", ".m4a", ".flac"));
        FileBrowser.SetDefaultFilter(".mp3");

        // Open the file dialog.
        FileBrowser.ShowLoadDialog(OnFileSelected, OnFileSelectionCanceled, FileBrowser.PickMode.Files, false, null, "Select a Song", "Select");
    }

    // Callback when a file is selected.
    private void OnFileSelected(string[] paths)
    {
        if (paths != null && paths.Length > 0)
        {
            string selectedPath = paths[0];
            UnityEngine.Debug.Log("Selected audio file: " + selectedPath);

            // Store the selected audio file path for later use.
            SelectedSongData.audioFilePath = selectedPath;

            // Now call the subprocess to run the "edit" executable.
            RunEditSubprocess(selectedPath);
        }
    }

    // Callback when file selection is canceled.
    private void OnFileSelectionCanceled()
    {
        UnityEngine.Debug.Log("Song selection canceled.");
    }

    // Calls the "edit" executable with --path and --output arguments.
    private void RunEditSubprocess(string songPath)
    {
        // Build the path to the "Scripts" folder inside StreamingAssets.
        string scriptsFolderPath = Path.Combine(Application.streamingAssetsPath, "Scripts");
        // Construct the full path to the executable (on macOS the executable likely has no extension)
        string exePath = Path.Combine(scriptsFolderPath, "AI-PatternGenerator");

        // Verify that the executable exists.
        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("Edit executable not found at: " + exePath);
            return;
        }

        // Build the output path: the Charts folder inside StreamingAssets.
        string chartsOutputPath = Path.Combine(Application.streamingAssetsPath, "Charts");

        // Build the command-line arguments.
        // Note: Surround paths with quotes in case they have spaces.
        string arguments = $"--path \"{songPath}\" --output \"{chartsOutputPath}\"";

        UnityEngine.Debug.Log("Running edit executable with arguments: " + arguments);

        // Setup ProcessStartInfo to run the executable.
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Start the process.
        Process process = new Process();
        process.StartInfo = psi;
        process.Start();

        // Read the output and error streams.
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        UnityEngine.Debug.Log("Edit process output: " + output);
        if (!string.IsNullOrEmpty(error))
        {
            UnityEngine.Debug.LogError("Edit process error: " + error);
        }
        SceneManager.LoadScene("MusicSelectedEditScene");
    }
}