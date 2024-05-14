using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointSelector : MonoBehaviour
{
    public GameObject objectToSelectFrom; // The object whose children will be considered
    public Camera mainCamera; // The main camera
    public TextMeshProUGUI coordinatesText;
    public TMP_Dropdown pointNameDropdown; // TMP dropdown for selecting point names
    public Button toggleButton; // Button to toggle point selection mode
    public GameObject pointIndicatorPrefab; // Prefab for the point indicator
    private bool isSelecting = false;

    // Dictionary to keep track of selected points by name
    private Dictionary<string, GameObject> selectedPoints = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Hook up button click event
        toggleButton.onClick.AddListener(ToggleSelectionMode);
    }

    private void ToggleSelectionMode()
    {
        isSelecting = !isSelecting;

        if (isSelecting)
        {
            Debug.Log("Point selection mode activated.");
        }
        else
        {
            Debug.Log("Point selection mode deactivated.");
        }
    }

    private void Update()
    {
        if (isSelecting && Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.transform.IsChildOf(objectToSelectFrom.transform))
                {
                    string selectedName = pointNameDropdown.options[pointNameDropdown.value].text;

                    if (selectedPoints.ContainsKey(selectedName))
                    {
                        Destroy(selectedPoints[selectedName]);
                        selectedPoints.Remove(selectedName);
                    }

                    GameObject selectedPoint = new GameObject(selectedName);

                    // Find the first child of objectToSelectFrom to use as the parent
                    Transform parentTransform = objectToSelectFrom.transform.GetChild(0);

                    // Set the selected point's parent to the first child of objectToSelectFrom
                    selectedPoint.transform.parent = parentTransform;

                    selectedPoint.transform.position = hit.point;

                    // Store the local position relative to the parent object
                    Vector3 localPosition = parentTransform.InverseTransformPoint(hit.point);
                    selectedPoint.transform.localPosition = localPosition;

                    // Instantiate the point indicator prefab
                    GameObject indicator = Instantiate(pointIndicatorPrefab, selectedPoint.transform);

                    // Ensure the indicator faces the camera initially
                    indicator.transform.LookAt(mainCamera.transform);

                    selectedPoints[selectedName] = selectedPoint;

                    Debug.Log("Selected point " + selectedName + " at " + hit.point);

                    ShowPointCoordinates();
                }
            }
        }

        // Update the rotation of all created point indicators based on the parent's rotation
        foreach (var kvp in selectedPoints)
        {
            GameObject pointObject = kvp.Value;
            GameObject indicator = pointObject.transform.GetChild(0).gameObject; // Assuming the indicator is the first child

            // Make the indicator face the camera
            indicator.transform.LookAt(mainCamera.transform);

            // Get the current rotation of the parent object (first child's rotation in this case)
            Quaternion parentRotation = objectToSelectFrom.transform.GetChild(0).rotation;

            // Apply the parent's rotation to the indicator
            indicator.transform.rotation = parentRotation;
        }
    }

    private void ShowPointCoordinates()
    {
        string coordinatesString = "Point Coordinates:\n";

        // Iterate over each selected point in the dictionary
        foreach (var kvp in selectedPoints)
        {
            string pointName = kvp.Key;
            GameObject pointObject = kvp.Value;

            // Get the local position relative to the parent object
            Vector3 localPosition = pointObject.transform.localPosition;

            // Transform the local position to world space
            Vector3 worldPosition = objectToSelectFrom.transform.TransformPoint(localPosition);

            // Append the point's name and coordinates to the coordinatesString
            coordinatesString += $"{pointName}: ({worldPosition.x:F2}, {worldPosition.y:F2}, {worldPosition.z:F2})\n";
        }

        // Update the coordinatesText with the formatted string
        coordinatesText.text = coordinatesString;
    }
}