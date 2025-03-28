using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    public static string DATA ="";
    public static string FILE_NAME = "log.txt";
    public static string hasSaved = "S\";
    void Start()
    {
        DATA = "Latency Config: \n" +
            "JITTER IN: " + NetworkManager.IN_JITTER + "\n" +
            "JITTER OUT: " + NetworkManager.OUT_JITTER + "\n" +
            "IN LOSS %: " + NetworkManager.IN_LOSS_CHANCE + "\n" +
            "OUT LOSS %: " + NetworkManager.OUT_LOSS_CHANCE + "\n";
    }

    public string GetQuestFilePath(string fileName)
    {
        return Path.Combine("/storage/emulated/0/Documents/", fileName);// Application.persistentDataPath
    }

    
    public void WriteToFile(string filePath)
    {
        try
        {
           
            File.WriteAllText(filePath, DATA);
            Debug.Log($"File written successfully at: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write to file: {e.Message}");
        }
    }
}
