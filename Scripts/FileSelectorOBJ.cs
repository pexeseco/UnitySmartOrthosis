using UnityEngine;
using UnityEditor;

public class FileSelector : MonoBehaviour
{
    public void SelectFile()
    {
        // Open file dialog
        string filePath = EditorUtility.OpenFilePanel("Select .obj File", "", "obj");

        if (!string.IsNullOrEmpty(filePath))
        {
            // Copy the file path to clipboard
            CopyToClipboard(filePath);
        }
        else
        {
            Debug.Log("File selection cancelled.");
        }
    }

    private void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
        Debug.Log("File path copied to clipboard: " + text);
    }
}