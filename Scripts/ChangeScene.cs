using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransferScript : MonoBehaviour
{
    public GameObject gameObjectToInstantiate;
    public Camera cameraToInstantiate;
    public Canvas canvasToInstantiate;

    public string newSceneName = "NewScene"; // Name of the new scene

    public Button instantiateButton; // Reference to the UI button

    void Start()
    {
        // Add a listener to the UI button
        instantiateButton.onClick.AddListener(OnInstantiateButtonClick);
    }

    void OnInstantiateButtonClick()
    {
        // Create a new scene
        SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);

        // Get the newly loaded scene
        Scene newScene = SceneManager.GetSceneByName(newSceneName);

        if (newScene.IsValid())
        {
            // Instantiate GameObject and all its children in the new scene
            GameObject newGameObject = InstantiateWithChildren(gameObjectToInstantiate);
            SceneManager.MoveGameObjectToScene(newGameObject, newScene);

            // Instantiate Camera in the new scene
            Camera newCamera = Instantiate(cameraToInstantiate);
            SceneManager.MoveGameObjectToScene(newCamera.gameObject, newScene);

            // Instantiate Canvas and all its children in the new scene
            Canvas newCanvas = InstantiateWithChildren(canvasToInstantiate);
            SceneManager.MoveGameObjectToScene(newCanvas.gameObject, newScene);
        }
        else
        {
            Debug.LogError("Failed to load the new scene: " + newSceneName);
        }
    }

    GameObject InstantiateWithChildren(GameObject original)
    {
        GameObject clone = Instantiate(original);

        // Recursively instantiate all child objects
        foreach (Transform child in original.transform)
        {
            GameObject childClone = InstantiateWithChildren(child.gameObject);
            childClone.transform.SetParent(clone.transform, worldPositionStays: false);
        }

        return clone;
    }

    Canvas InstantiateWithChildren(Canvas original)
    {
        Canvas clone = Instantiate(original);

        // Recursively instantiate all child elements of the Canvas
        foreach (Transform child in original.transform)
        {
            GameObject childClone = InstantiateWithChildren(child.gameObject);
            childClone.transform.SetParent(clone.transform, worldPositionStays: false);
        }

        return clone;
    }
}