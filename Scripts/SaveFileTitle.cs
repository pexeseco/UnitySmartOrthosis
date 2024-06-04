using UnityEngine;
using TMPro;

public class SaveData : MonoBehaviour
{
    public TMP_InputField inputField1;
    public TMP_InputField inputField2;
    public TMP_InputField inputField3;
    public TextMeshProUGUI displayTextMeshPro; // Reference to TextMeshPro object to display saved text1

    private string text1; // Store text1
    private string text2; // Store text2
    private string text3; // Store text3

    public void SaveDataInMemory()
    {
        // Get the inputs from the input fields
        text1 = inputField1.text;
        text2 = inputField2.text;
        text3 = inputField3.text;

        // Display a message indicating the data is saved
        Debug.Log("Data saved in memory.");

        // Update the TextMeshPro component with the saved text1
        if (displayTextMeshPro != null)
        {
            displayTextMeshPro.text = text1;
        }
        else
        {
            Debug.LogWarning("TextMeshPro component not assigned for displaying text1.");
        }
    }

    // Method to access the saved text1
    public string GetText1()
    {
        return text1;
    }

    // Method to access the saved text2
    public string GetText2()
    {
        return text2;
    }

    // Method to access the saved text3
    public string GetText3()
    {
        return text3;
    }
}