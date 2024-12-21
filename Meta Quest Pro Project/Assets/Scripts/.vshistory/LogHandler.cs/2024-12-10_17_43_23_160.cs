using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    private string data;
    void Start()
    {
        string fileName = "log.txt";

        string filePath = GetQuestFilePath(fileName);

        WriteToFile(filePath, data);
    }

    private string GetQuestFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    
    private void WriteToFile(string filePath, string data)
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
