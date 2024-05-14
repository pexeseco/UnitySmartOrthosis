using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelClickRaycast : MonoBehaviour
{
    public Camera raycastCamera; // Reference to the camera for raycasting
    public string targetObjectName; // Name of the target object to interact with
    public TMP_Dropdown dropdown; // Reference to the TMP dropdown for selecting point name
    public Button toggleButton; // Reference to the toggle button

    private bool isActivated = false; // Flag to track activation state
    private string pointName = ""; // Name of the point to be refreshed, initially empty

    void Start()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleActivation);
        }
    }

    void Update()
    {
        if (isActivated)
        {
            // Check if the left mouse button is clicked
            if (Input.GetMouseButtonDown(0))
            {
                // Check if the mouse click is within the boundaries of the panel
                if (RectTransformUtility.RectangleContainsScreenPoint(
                        GetComponent<RectTransform>(), Input.mousePosition))
                {
                    // Perform raycast from the camera at the calculated world position
                    RaycastHit hit;
                    Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

                        // Check if the hit object or any of its parents match the target object name
                        if (IsTargetObject(hit.collider.gameObject.transform))
                        {
                            GameObject existingPoint = GameObject.Find(pointName);
                            if (existingPoint != null)
                            {
                                existingPoint.transform.position = hit.point;
                                Debug.Log("Point " + pointName + " refreshed at: " + hit.point);

                                // Log a debug message for hitting the target object
                                Debug.Log("Target Object (" + targetObjectName + ") selected!");
                            }
                            else
                            {
                                Debug.LogError("Point " + pointName + " not found!");
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsTargetObject(Transform objTransform)
    {
        // Traverse up the parent hierarchy to check if any parent's name matches the target object name
        Transform currentTransform = objTransform;
        int maxDepth = 10; // Maximum depth to traverse (adjust as needed)

        for (int i = 0; i < maxDepth; i++)
        {
            if (currentTransform == null)
                break;

            if (currentTransform.gameObject.name == targetObjectName)
                return true;

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    void OnDropdownValueChanged(int value)
    {
        if (dropdown != null && value >= 0 && value < dropdown.options.Count)
        {
            pointName = dropdown.options[value].text;
            Debug.Log("Selected Point Name: " + pointName);
        }
    }

    void ToggleActivation()
    {
        isActivated = !isActivated; // Toggle the activation state
        Debug.Log("Raycast Activation: " + isActivated);
    }
}