using UnityEngine;
using System.IO;
using System.Linq;
using Dummiesman;

public class ObjFromFile : MonoBehaviour
{
    public GameObject parentObject; // Reference to the empty GameObject that will be the 
    string objPath = string.Empty;
    string error = string.Empty;

    void OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float textFieldHeight = 32f;
        float buttonWidth = 80f; // Width of the "Load File" button
        float errorBoxHeight = 32f;

        // Calculate Y positions for GUI elements
        float textFieldY = screenHeight - textFieldHeight;
        float buttonY = screenHeight - textFieldHeight - buttonWidth;
        float errorBoxY = screenHeight - textFieldHeight - buttonWidth - errorBoxHeight;

        // Calculate width of "Obj Path:" label
        float labelWidth = GUI.skin.label.CalcSize(new GUIContent("Obj Path:")).x;

        // Draw "Obj Path:" label
        GUI.Label(new Rect(0, textFieldY, labelWidth, 32), "Obj Path:");

        // Draw text field for entering file path
        objPath = GUI.TextField(new Rect(labelWidth, textFieldY, 256 - labelWidth, 32), objPath);

        // Draw "Load File" button to the right of the label
        if (GUI.Button(new Rect(labelWidth + 10, buttonY, buttonWidth, 32), "Load File"))
        {
            LoadObjectFromFile();
        }

        // Draw error message box
        if (!string.IsNullOrWhiteSpace(error))
        {
            GUI.color = Color.red;
            GUI.Box(new Rect(0, errorBoxY, 256 + 64, 32), error);
            GUI.color = Color.white;
        }
    }

    void LoadObjectFromFile()
    {
        if (!File.Exists(objPath))
        {
            error = "File doesn't exist.";
        }
        else
        {
            // Load the OBJ file and instantiate it in the scene
            GameObject objPrefab = new OBJLoader().Load(objPath);

            if (objPrefab != null)
            {
                // Rotate and position the object as you did before
                objPrefab.transform.Rotate(0f, 180f, 0f);
                objPrefab.transform.Translate(new Vector3(-1530f, 30f, -300f));

                // Check if parentObject reference is assigned and set parent accordingly
                if (parentObject != null)
                {
                    objPrefab.transform.SetParent(parentObject.transform);
                }
                else
                {
                    Debug.LogWarning("ParentObject is not assigned. Object will be parented to this GameObject.");
                    objPrefab.transform.SetParent(this.transform);
                }

                // Change the material of the loaded object to defaultMat
                Material defaultMat = Resources.Load<Material>("HeadMaterial");
                if (defaultMat != null)
                {
                    Renderer[] renderers = objPrefab.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        Material[] materials = renderer.materials;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            materials[i] = defaultMat;
                        }
                        renderer.materials = materials;
                    }
                }
                else
                {
                    Debug.LogError("Failed to find default material named 'defaultMat'. Make sure it exists in the project's assets.");
                }

                // Add MeshCollider as before
                Transform childWithMesh = FindChildWithMesh(objPrefab.transform);
                if (childWithMesh != null)
                {
                    MeshCollider meshCollider = childWithMesh.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = false;
                }

                error = string.Empty;
            }
            else
            {
                error = "Failed to load object.";
            }
        }
    }

    // Helper method to recursively find a child with a MeshFilter component
    private Transform FindChildWithMesh(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Check if the child has a MeshFilter component
            if (child.GetComponent<MeshFilter>() != null)
            {
                return child; // Found a child with a MeshFilter, return it
            }

            // Recursively search deeper in the hierarchy
            Transform foundChild = FindChildWithMesh(child);
            if (foundChild != null)
            {
                return foundChild; // Found a suitable child deeper in the hierarchy
            }
        }

        return null; // No child with a MeshFilter component found
    }
}