using System.IO;
using TMPro;
using UnityEngine;

public class ExportToCSV : MonoBehaviour
{
    public TMP_Text contentTextMeshPro;
    public SaveData saveData;

    private readonly string[] measurements = {
        "Cranial Base Difference",
        "Circumference",
        "Cranial Width",
        "Cranial Length",
        "CR",
        "CVA",
        "Overall Symmetry Ratio",
        "Diagonal1",
        "Diagonal2"
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
        string fileName = GenerateFileName(text1, text2);

        // Check if filename is not empty
        if (!string.IsNullOrEmpty(fileName))
        {
            // Generate CSV file path based on saved filename
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".csv");

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
        }

        Debug.Log("CSV file saved to: " + filePath);
    }

    private string GenerateFileName(string text1, string text2)
    {
        // Generate a filename based on text1
        return $"{text1}";
    }
}