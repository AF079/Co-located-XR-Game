using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    void Start()
    {
        // Example data to save
        string fileName = "log.txt";

        // Get the path to save the file
        string filePath = GetQuestFilePath(fileName);

        // Write data to the file
        WriteToFile(filePath, dataToWrite);
    }

    // Method to construct a file path for Meta Quest
    private string GetQuestFilePath(string fileName)
    {
        // On Quest, Application.persistentDataPath points to an accessible storage location
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    // Method to write data to the file
    private void WriteToFile(string filePath, string data)
    {
        try
        {
            // Write the data to the file
            File.WriteAllText(filePath, data);
            Debug.Log($"File written successfully at: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write to file: {e.Message}");
        }
    }
}
