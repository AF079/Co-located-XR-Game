using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    public static string DATA ="";
    public static string FILE_NAME = "log.txt";
    public static string hasSaved = "";
    void Start()
    {
        DATA = "Latency Config: \n" +
            "SERVER TO CLIENT LATENCY: " + NetworkManager.CLIENT_LATENCY + "\n";
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
