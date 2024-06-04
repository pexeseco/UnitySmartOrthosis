using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class PointPlacementOnObjectSurface : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject targetEmptyObject;
    public GameObject specificPoint;
    public Canvas canvas; // Reference to the canvas for TextMeshPro

    public TMP_Dropdown pointDropdown;


    private Vector3[] pointPositions;
    private string[] pointNames = { "SL", "r-TR", "l-TR" };
    private int selectedPointIndex = 0;
    
    private void Start()
    {
        if (pointDropdown != null)
        {
            pointDropdown.ClearOptions();
            pointDropdown.AddOptions(pointNames.ToList());
            pointDropdown.onValueChanged.AddListener(OnPointSelected);
        }

    }

    public void PlacePointsOnObjectSurface()
    {
        // Initialize point positions array
        pointPositions = new Vector3[pointNames.Length];

        pointPositions[0] = new Vector3(0f, 0f, 0f); // Example position for point "SL"
        pointPositions[1] = new Vector3(1f, 0f, 0f); // Example position for point "r-TR"
        pointPositions[2] = new Vector3(-1f, 0f, 0f); // Example position for point "l-TR"

        if (pointPrefab == null || targetEmptyObject == null || canvas == null || pointDropdown == null )
        {
            Debug.LogError("Please assign all required objects in the inspector.");
            return;
        }

        ClearExistingPoints();

        MeshFilter meshFilter = targetEmptyObject.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("No MeshFilter component or mesh found in the targetEmptyObject hierarchy.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        if (vertices.Length == 0)
        {
            Debug.LogError("Mesh has no vertices.");
            return;
        }

        // Find min and max X and Y values
        float minX = vertices[0].x;
        float maxX = vertices[0].x;
        float minY = vertices[0].y;
        float maxY = vertices[0].y;

        foreach (Vector3 vertex in vertices)
        {
            minX = Mathf.Min(minX, vertex.x);
            maxX = Mathf.Max(maxX, vertex.x);
            minY = Mathf.Min(minY, vertex.y);
            maxY = Mathf.Max(maxY, vertex.y);
        }

        // Calculate center position
        Vector3 centerPosition = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, vertices[0].z);

        // Calculate positions for point 2 and point 3
        Vector3 point2Position = new Vector3(centerPosition.x + 5f, centerPosition.y, centerPosition.z);
        Vector3 point3Position = new Vector3(centerPosition.x - 5f, centerPosition.y, centerPosition.z);

        // Create and position points
        for (int i = 0; i < pointPositions.Length; i++)
        {
            CreatePoint(pointPositions[i], pointNames[i]);
        }

        
        MoveSpecificPointToMeshCenter();
    }

    

    private void OnPointSelected(int index)
    {
        selectedPointIndex = index;
    }


    private void CreatePoint(Vector3 position, string pointName)
    {
        GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
        point.transform.SetParent(transform);

        MeshFilter meshFilter = targetEmptyObject.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("No MeshFilter component or mesh found in the targetEmptyObject hierarchy.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Vector3 surfaceNormal = Vector3.zero;

        // Find the average surface normal at the given position
        foreach (var triIndex in Enumerable.Range(0, triangles.Length / 3))
        {
            Vector3 v1 = vertices[triangles[triIndex * 3]];
            Vector3 v2 = vertices[triangles[triIndex * 3 + 1]];
            Vector3 v3 = vertices[triangles[triIndex * 3 + 2]];

            // Calculate the face normal of the triangle
            Vector3 triangleNormal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            // Check if the triangle's normal is pointing towards the point
            if (Vector3.Dot(triangleNormal, position - v1) > 0)
            {
                surfaceNormal += triangleNormal;
            }
        }

        surfaceNormal = surfaceNormal.normalized;

        float offset = 0.2f; // Offset distance from the mesh surface
        position += surfaceNormal * offset;

        point.transform.position = position;

        // Create a text object for the point
        GameObject textObject = new GameObject("PointNameText");
        TextMeshProUGUI pointNameText = textObject.AddComponent<TextMeshProUGUI>();
        pointNameText.text = pointName;
        pointNameText.fontSize = 10;
        pointNameText.alignment = TextAlignmentOptions.Center;
        pointNameText.color = Color.red;
        pointNameText.fontStyle = FontStyles.Bold;

        // Parent the text object to the canvas
        textObject.transform.SetParent(canvas.transform, false);

        // Position the text object in screen space based on the point's position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);
        Vector3 canvasCenter = new Vector3(canvas.pixelRect.center.x, canvas.pixelRect.center.y, 0f);
        textObject.GetComponent<RectTransform>().anchoredPosition = screenPos - canvasCenter;
    }

    private void ClearExistingPoints()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }



    private void MoveSpecificPointToMeshCenter()
    {
        if (specificPoint == null || targetEmptyObject == null)
        {
            Debug.LogWarning("Specific point or targetEmptyObject not assigned.");
            return;
        }

        MeshFilter meshFilter = targetEmptyObject.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("No MeshFilter component or mesh found in the targetEmptyObject hierarchy.");
            return;
        }

        Vector3[] vertices = meshFilter.sharedMesh.vertices;
        Vector3 meshCenter = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            meshCenter += vertex;
        }
        meshCenter /= vertices.Length;

        specificPoint.transform.position = meshCenter;
    }


    public void UpdatePointPosition(int index)
    {
        if (index >= 0 && index < transform.childCount)
        {
            Transform point = transform.GetChild(index);

            RaycastHit hit;
            if (Physics.Raycast(point.position, -Vector3.up, out hit))
            {
                point.position = hit.point;
                pointPositions[index] = hit.point;
            }

        }
    }

}
