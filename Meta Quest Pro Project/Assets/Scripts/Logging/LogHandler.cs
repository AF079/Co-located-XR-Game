using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    private string fileName = "LATENCY_LOG.txt";
    public static string LOG = "";


    void Start()
    {
        LOG = string.Format("{0,-10} | {1,-12} | {2,-14:F3} | {3,-10} | {4,-12}\n",
                                     "Sphere Nr", "Timestamp", "RTT", "Ticks", "Color");
        SaveText(LOG);
    }

    public void SaveText(string content)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, content);
        Debug.Log($"File saved at: {path}");
    }

    public string LoadText()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            Debug.Log($"File loaded: {content}");
            return content;
        }
        else
        {
            Debug.LogWarning("File not found!");
            return null;
        }
    }
}