using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlaneCreator : MonoBehaviour
{
    public delegate void PlaneSpawnedEvent();
    public static event PlaneSpawnedEvent OnPlaneSpawned;

    public GameObject targetObject; // The parent object whose children will be searched for SpriteRenderer components
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
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned!");
            return;
        }

        SpriteRenderer[] spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length < 3)
        {
            Debug.LogWarning("Not enough sprite renderers found!");
            return;
        }

        // Find the child object with a MeshFilter
        MeshFilter[] meshFilters = targetObject.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("No MeshFilter found in children objects!");
            return;
        }
        Mesh mesh = meshFilters[0].sharedMesh; // Assuming only one MeshFilter for simplicity

        // Delete previous planes if they exist
        foreach (Transform plane in createdPlanes)
        {
            Destroy(plane.gameObject);
        }
        createdPlanes.Clear(); // Clear the list

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
            Transform currentPlane = Instantiate(planePrefab, targetObject.transform);
            currentPlane.name = "plane" + (++planeCount);
            currentPlane.localPosition = planePosition;
            currentPlane.localRotation = Quaternion.LookRotation(planeNormal);
            createdPlanes.Add(currentPlane); // Add the created plane to the list

            // Scale the plane to fit between the three points
            Vector3 size = new Vector3(Vector3.Distance(positions[0], positions[1]) * 1.75f,
                                Vector3.Distance(positions[0], positions[2]) * 1.25f,
                                0.1f);
            currentPlane.localScale = size * 2.5f;

            // Dispatch the event indicating that a new plane has been spawned
            OnPlaneSpawned?.Invoke();

            // Add the plane to dropdown options
            planeOptions.Add(currentPlane.name);
        }

        // Set the dropdown options
        planeDropdown.AddOptions(planeOptions);
    }

    private Vector3 FindFarthestPoint(Vector3 planePosition, Mesh mesh)
    {
        Vector3 farthestPoint = Vector3.zero;
        float maxDistance = float.MinValue;
        float maxY = float.MinValue;

        // Get the positions of the sprite renderers
        SpriteRenderer[] spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>();
        Vector3[] positions = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            positions[i] = spriteRenderers[i].transform.position;
        }

        // Calculate the plane's normal
        Vector3 planeNormal = Vector3.Cross(positions[1] - positions[0], positions[2] - positions[0]).normalized;

        foreach (Vector3 vertex in mesh.vertices)
        {
            // Transform vertex position to world space
            Vector3 worldVertex = targetObject.transform.TransformPoint(vertex);

            // Project the vertex onto the plane
            Vector3 projectedVertex = worldVertex - Vector3.Dot(worldVertex - planePosition, planeNormal) * planeNormal;

            // Calculate distance from the projected point to the vertex
            float distance = Vector3.Distance(worldVertex, projectedVertex);

            // Check if the vertex is above the plane
            if (worldVertex.y > planePosition.y)
            {
                // Update farthest point if distance is greater than previous max and y value is higher
                if (distance > maxDistance && worldVertex.y > maxY)
                {
                    maxDistance = distance;
                    maxY = worldVertex.y;
                    farthestPoint = worldVertex;
                }
            }
        }

        return farthestPoint;
    }
}