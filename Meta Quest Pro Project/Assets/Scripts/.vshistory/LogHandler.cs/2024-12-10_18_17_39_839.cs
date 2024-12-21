using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    public string data;
    void Start()
    {
        data = "Latency Config: \n" +
            "JITTER IN: " + NetworkManager.IN_JITTER + "\n" +
            "JITTER OUT: " + NetworkManager.OUT_JITTER + "\n" +
            "IN LOSS %: " + NetworkManager.IN_LOSS_CHANCE + "\n" +
            "OUT LOSS %: " + NetworkManager.OUT_LOSS_CHANCE + "\n";
    }

    public string GetQuestFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    
    public void WriteToFile(string filePath, string data)
    {
        try
        {
           
            File.WriteAllText(filePath, data);
            Debug.Log($"File written successfully at: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write to file: {e.Message}");
        }
    }
}
