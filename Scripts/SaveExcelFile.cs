using System.IO;
using TMPro;
using UnityEngine;

public class ExportToCSV : MonoBehaviour
{
    public TMP_Text contentTextMeshPro;
    public SaveData saveData;

    private readonly string[] measurements = {
        "Circumference",
        "Cranial Width",
        "Cranial Length",
        "Diagonal1",
        "Diagonal2",
        "CVA",
        "CR",
        "Plane"
    };

    private void Start()
    {
        // Call Export method on Start for demonstration
        Export();
    }

    public void Export()
    {
        // Get saved data from SaveData script
        string text1 = saveData.GetText1();
        string text2 = saveData.GetText2();
        string text3 = saveData.GetText3();

        // Prepare content for CSV
        string content = $"{text1}\n{text2}\n{text3}";

        // Generate a filename for the CSV
        string fileName = GenerateFileName(text1);

        // Check if filename is not empty
        if (!string.IsNullOrEmpty(fileName))
        {
            // Define the data folder path (assuming the folder is named "Data" and already exists)
            string dataFolderPath = Path.Combine(Application.persistentDataPath, "Data");

            // Generate CSV file path based on saved filename inside the data folder
            string filePath = Path.Combine(dataFolderPath, fileName + ".csv");

            // Write content to CSV file
            WriteToCSV(filePath, content);
        }
        else
        {
            Debug.LogError("Failed to generate filename.");
        }
    }

    private void WriteToCSV(string filePath, string content)
    {
        // Write to the CSV file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write headers (measurement names)
            writer.Write("\"Text2\",");
            foreach (string measurement in measurements)
            {
                writer.Write($"\"{measurement}\",");
            }
            writer.WriteLine();

            // Write content
            writer.Write("\"Text3\",");
            writer.WriteLine(content);
            writer.Write("\"Text4\",");
        }

        Debug.Log("CSV file saved to: " + filePath);
    }

    private string GenerateFileName(string text1)
    {
        // Generate a filename based on text1
        return $"{text1}";
    }
}