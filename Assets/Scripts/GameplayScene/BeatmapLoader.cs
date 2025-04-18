using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BeatmapLoader : MonoBehaviour
{
    // Loads a beatmap from the given file path.
    public Beatmap LoadBeatmap(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Beatmap file not found: " + filePath);
            return null;
        }

        string[] lines = File.ReadAllLines(filePath);
        Beatmap beatmap = new Beatmap();
        bool inTimingPoints = false;
        bool inHitObjects = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.Length == 0) continue;

            // Check for section headers.
            if (line.StartsWith("["))
            {
                inTimingPoints = line.Equals("[TimingPoints]");
                inHitObjects = line.Equals("[HitObjects]");
                continue;
            }

            if (inTimingPoints)
            {
                // Expect a line like: "128,380.226757"
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    float timeMs, beatLength;
                    if (float.TryParse(parts[0], out timeMs) && float.TryParse(parts[1], out beatLength))
                    {
                        // Use the first timing point.
                        // (If needed, you can store an offset from this first value.)
                        beatmap.TimingPointTime = timeMs / 1000f; // Convert to seconds (may be used for an offset)
                        beatmap.BPM = 60000f / beatLength; // Calculate BPM from beatLength in ms.
                        inTimingPoints = false; // For simplicity, we use one timing point.
                    }
                }
            }

            if (inHitObjects)
            {
                // Expect a line like: "128,_ _ O _" where the first token is time in ms.
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    float timeMs;
                    if (float.TryParse(parts[0], out timeMs))
                    {
                        string laneData = parts[1].Trim();
                        // Split by space and remove empty entries.
                        string[] tokens = laneData.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        // We expect 4 tokens, one per lane.
                        for (int i = 0; i < tokens.Length && i < 4; i++)
                        {
                            if (tokens[i] == "O")
                            {
                                HitObject hit = new HitObject();
                                hit.Time = timeMs / 1000f; // Convert ms to seconds.
                                hit.Lane = i;
                                beatmap.HitObjects.Add(hit);
                            }
                        }
                    }
                }
            }
        }

        // Sort the hit objects by time.
        beatmap.HitObjects.Sort((a, b) => a.Time.CompareTo(b.Time));
        return beatmap;
    }
}

public class Beatmap
{
    public float BPM;
    public float TimingPointTime;
    public List<HitObject> HitObjects = new List<HitObject>();
}

public class HitObject
{
    public int Lane;
    public float Time; // In seconds.
}