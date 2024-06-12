using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SaveRawImage : MonoBehaviour
{
    public RawImage rawImage;
    public TMP_Text fileNameText;
    public TMP_InputField additionalNameInput;
    public TMP_Dropdown nameDropdown;

    public void SaveImage()
    {
        if (rawImage.texture == null || string.IsNullOrEmpty(fileNameText.text))
        {
            Debug.LogWarning("RawImage or fileNameText is not set properly.");
            return;
        }

        Texture2D texture = rawImage.texture as Texture2D;
        if (texture == null)
        {
            Debug.LogError("RawImage texture is not a Texture2D.");
            return;
        }

        // Create a new Texture2D to copy the rawImage texture
        Texture2D newTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        newTexture.SetPixels(texture.GetPixels());
        newTexture.Apply();

        byte[] bytes = newTexture.EncodeToPNG();
        Destroy(newTexture);

        // Define the save path
        string folderPath = Application.dataPath + "/SavedImages";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Construct the filename
        string baseName = fileNameText.text;
        string additionalName = additionalNameInput.text;
        string dropdownOption = nameDropdown.options[nameDropdown.value].text;
        string finalFileName = $"{baseName}_{additionalName}_{dropdownOption}.png";

        string filePath = Path.Combine(folderPath, finalFileName);

        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Image saved to: {filePath}");
    }
}