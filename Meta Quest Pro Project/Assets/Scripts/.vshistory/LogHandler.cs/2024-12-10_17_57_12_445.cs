using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    public string data;
    void Start()
    {
        string fileName = "log.txt";

        string filePath = GetQuestFilePath(fileName);

        WriteToFile(filePath, data);
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
