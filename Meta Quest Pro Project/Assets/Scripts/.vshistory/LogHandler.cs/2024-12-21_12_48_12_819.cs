using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{

    //Data that will be stored in log file
    public static string DATA ="";
    //File name
    public static string FILE_NAME = "log.txt";
    void Start()
    {
        //Before logging, add the latency level
        DATA = "Latency Config: \n" +
            "SERVER TO CLIENT LATENCY: " + NetworkManager.CLIENT_LATENCY + "\n";
    }

    //Get filepath
    public string GetQuestFilePath(string fileName)
    {
        return Path.Combine("/storage/emulated/0/Documents/", fileName);
    }

    
    //Function to write data to file path
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
