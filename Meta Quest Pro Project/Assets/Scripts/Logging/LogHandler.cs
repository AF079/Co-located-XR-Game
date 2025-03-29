using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    private string fileName = "MyTextFile.txt";

    void Start()
    {
        Debug.Log("SAVING");
        SaveText("THIS IS DATA :)");
        Debug.Log("DONE SAVING");
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