using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointSelector : MonoBehaviour
{
    public Camera mainCamera; // The main camera
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
                if (hit.collider.gameObject.name == "Object")
                {
                    string selectedName = pointNameDropdown.options[pointNameDropdown.value].text;

                    if (selectedPoints.ContainsKey(selectedName))
                    {
                        Destroy(selectedPoints[selectedName]);
                        selectedPoints.Remove(selectedName);
                    }

                    GameObject selectedPoint = new GameObject(selectedName);

                    // Set the selected point's parent to the object named "Object"
                    selectedPoint.transform.parent = hit.collider.gameObject.transform;

                    selectedPoint.transform.position = hit.point;

                    // Instantiate the point indicator prefab
                    GameObject indicator = Instantiate(pointIndicatorPrefab, selectedPoint.transform);

                    // Ensure the indicator faces the camera initially
                    indicator.transform.LookAt(mainCamera.transform);

                    selectedPoints[selectedName] = selectedPoint;

                    Debug.Log("Selected point " + selectedName + " at " + hit.point);
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
        }
    }
}