using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlaneCreator : MonoBehaviour
{
    public delegate void PlaneSpawnedEvent();
    public static event PlaneSpawnedEvent OnPlaneSpawned;

    public Transform planePrefab; // The prefab of the plane to be created
    public Button searchButton; // The button to trigger the search
    public TMP_Dropdown planeDropdown; // TMP_Dropdown to show the created planes

    private List<Transform> createdPlanes = new List<Transform>(); // List to hold references to created planes
    private int planeCount = 0;

    private void Start()
    {
        // Attach the button click listener
        searchButton.onClick.AddListener(SearchForSpritesAndCreatePlane);
    }

    private void SearchForSpritesAndCreatePlane()
    {
        GameObject targetObject = GameObject.Find("Object");
        if (targetObject == null)
        {
            Debug.LogError("Target object named 'Object' not found in the scene!");
            return;
        }

        SpriteRenderer[] spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length < 3)
        {
            Debug.LogWarning("Not enough sprite renderers found!");
            return;
        }

        // Delete previous planes if they exist
        foreach (Transform plane in createdPlanes)
        {
            Destroy(plane.gameObject);
        }
        createdPlanes.Clear(); // Clear the list

        // Reset the plane count
        planeCount = 0;

        // Create the planes
        Vector3[] positions = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            positions[i] = spriteRenderers[i].transform.position;
        }

        // Calculate the distance between planes
        float distanceBetweenPlanes = Vector3.Distance(positions[0], positions[1]) / 8.0f;

        // Clear existing options
        planeDropdown.ClearOptions();

        // Create the planes
        List<string> planeOptions = new List<string>();
        for (int j = 0; j < 12; j++) // Create 12 planes
        {
            // Calculate the plane position based on the distance between planes
            Vector3 planePosition;
            if (j < 7)
            {
                planePosition = positions[0] + Vector3.up * j * distanceBetweenPlanes;
            }
            else
            {
                planePosition = positions[0] - Vector3.up * (12 - j) * distanceBetweenPlanes;
            }

            // Calculate the plane's normal
            Vector3 planeNormal = Vector3.Cross(positions[1] - positions[0], positions[2] - positions[0]).normalized;

            // Create the plane
            Transform currentPlane = Instantiate(planePrefab, planePosition, Quaternion.LookRotation(planeNormal));
            currentPlane.name = "plane" + (++planeCount);
            createdPlanes.Add(currentPlane); // Add the created plane to the list

            // Scale the plane to fit between the three points
            Vector3 size = new Vector3(Vector3.Distance(positions[0], positions[1]) * 1.75f,
                                       Vector3.Distance(positions[0], positions[2]) * 1.25f,
                                       0.01f);

            Debug.Log($"Scaling plane {currentPlane.name} with size {size}");
            currentPlane.localScale = size * 2.5f;

            // Dispatch the event indicating that a new plane has been spawned
            OnPlaneSpawned?.Invoke();

            // Add the plane to dropdown options
            planeOptions.Add(currentPlane.name);
        }

        // Calculate scale for the last two planes
        Vector3 scale = createdPlanes[0].localScale;

        // Create the second-to-last plane
        Transform firstPlane = createdPlanes[0];
        Transform secondToLastPlane = Instantiate(planePrefab, targetObject.transform);
        secondToLastPlane.name = "plane" + (++planeCount);
        secondToLastPlane.position = firstPlane.position;
        secondToLastPlane.rotation = firstPlane.rotation * Quaternion.Euler(90, 0, 0); // Rotate 90 degrees on X axis

        Transform lTrSprite = targetObject.transform.Find("l-TR");
        Transform rTrSprite = targetObject.transform.Find("r-TR");
        if (lTrSprite != null && rTrSprite != null)
        {
            // Adjust the position of the second-to-last plane to intersect the midway point between l-TR and r-TR
            secondToLastPlane.position = (lTrSprite.position + rTrSprite.position) / 2.0f;
        }
        createdPlanes.Add(secondToLastPlane); // Add the created plane to the list
        planeOptions.Add(secondToLastPlane.name);
        secondToLastPlane.localScale = scale;

        // Create the last plane
        Transform lastPlane = Instantiate(planePrefab, targetObject.transform);
        lastPlane.name = "plane" + (++planeCount);
        lastPlane.position = secondToLastPlane.position;
        lastPlane.rotation = secondToLastPlane.rotation * Quaternion.Euler(0, 90, 0); // Rotate 90 degrees on Y axis

        Transform slSprite = targetObject.transform.Find("SL");
        if (rTrSprite != null && slSprite != null)
        {
            // Adjust the position of the last plane to intersect with SL
            Vector3 avgPositionTR = (rTrSprite.position + lTrSprite.position) / 2.0f;
            lastPlane.position = (avgPositionTR + slSprite.position) / 2.0f;
        }

        createdPlanes.Add(lastPlane); // Add the created plane to the list
        planeOptions.Add(lastPlane.name);
        lastPlane.localScale = scale;

        // Set the dropdown options
        planeDropdown.AddOptions(planeOptions);
    }
}