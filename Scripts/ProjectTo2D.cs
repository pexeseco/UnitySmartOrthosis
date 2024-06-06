using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TMPro;

public class IntersectionProjection : MonoBehaviour
{
    public RectTransform panel; // Reference to the UI panel where the 2D plane will be represented
    public GameObject pointPrefab; // Prefab of the UI element representing a projected point
    public Material lineMaterial; // Material for the line renderer
    public Material circleMaterial; // Material for the circle renderer
    public RawImage imageDisplay; // RawImage UI element to display the rendered texture
    public TMP_Text textDisplay; // Reference to the TMP Text UI element for displaying messages

    // Custom class to hold point and angle information
    class PointWithAngle
    {
        public GameObject Point { get; set; }
        public float Angle { get; set; }
    }


    public void ProjectTo2D()
    {
        // Find all objects with the tag "IntersectionObject"
        GameObject[] intersectionObjects = GameObject.FindGameObjectsWithTag("IntersectionObject");

        // Find the object with the tag "AveragePoint"
        GameObject averagePointObject = GameObject.FindGameObjectWithTag("AveragePoint");

        // Check if necessary objects are found
        if (intersectionObjects.Length == 0 || averagePointObject == null || panel == null || pointPrefab == null)
        {
            Debug.LogWarning("Not all necessary objects found!");
            return;
        }

        // Remove the average point object from the list of intersection objects
        intersectionObjects = intersectionObjects.Where(obj => obj != averagePointObject).ToArray();

        // Reference to the average point transform
        Transform averagePoint = averagePointObject.transform;

        // Ensure we have at least one intersection object after removing the average point
        if (intersectionObjects.Length < 1)
        {
            Debug.LogWarning("At least one intersection object is required for projection!");
            return;
        }

        List<GameObject> projectedPoints = new List<GameObject>();

        // Project each intersection object onto the 2D plane
        foreach (GameObject intersectionObject in intersectionObjects)
        {
            // Get the position of the intersection point
            Vector3 intersectionPosition = intersectionObject.transform.position;

            // Calculate the distance vector between the average point and the intersection point in the XY plane
            Vector3 distanceVector = intersectionPosition - averagePoint.position;

            // Scale up the distance vector by 1.5
            distanceVector *= 1.5f;

            // Project the distance vector onto the XY plane using the normal vector
            Vector2 projectedPoint = new Vector2(Vector3.Dot(distanceVector, averagePoint.right), Vector3.Dot(distanceVector, averagePoint.forward));

            // Convert the projected point from local space to screen space
            Vector2 screenPoint = new Vector2(projectedPoint.x, projectedPoint.y);

            // Create a new UI element (point) representing the projected point
            GameObject point = Instantiate(pointPrefab, panel);

            // Set the name of the UI element to the name of the IntersectionObject
            point.name = intersectionObject.name + "Projected";

            // Set the position of the UI element within the panel
            RectTransform pointTransform = point.GetComponent<RectTransform>();
            pointTransform.anchoredPosition = screenPoint + panel.sizeDelta / 2f; // Adjust position relative to panel center

            projectedPoints.Add(point);
        }

        // Create a new UI element (point) representing the average point
        GameObject avgPointDraw = Instantiate(pointPrefab, panel);
        avgPointDraw.name = "avgPointDraw";
        RectTransform avgPointTransform = avgPointDraw.GetComponent<RectTransform>();
        avgPointTransform.anchoredPosition = panel.sizeDelta / 2f; // Position at the center of the panel


        // Calculate the angle of each point relative to the average point and sort them
        Vector2 averagePointScreenPosition = panel.sizeDelta / 2f; // Assuming averagePoint is at the center of the panel
        var pointsWithAngles = projectedPoints.Select(point => new PointWithAngle
        {
            Point = point,
            Angle = Mathf.Atan2(point.GetComponent<RectTransform>().anchoredPosition.y - averagePointScreenPosition.y,
                                point.GetComponent<RectTransform>().anchoredPosition.x - averagePointScreenPosition.x)
        }).OrderBy(pa => pa.Angle).ToList();

        // Create a LineRenderer component for the existing line
        GameObject lineObject = new GameObject("LineRenderer");
        lineObject.transform.SetParent(panel.transform, false);
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.widthMultiplier = 3f;
        lineRenderer.useWorldSpace = false; // Ensure it uses the panel's local space

        // Set the number of positions in the LineRenderer for the existing line
        lineRenderer.positionCount = pointsWithAngles.Count;

        // Set the positions of the LineRenderer for the existing line
        for (int i = 0; i < pointsWithAngles.Count; i++)
        {
            Vector2 pointPosition = pointsWithAngles[i].Point.GetComponent<RectTransform>().anchoredPosition;
            lineRenderer.SetPosition(i, new Vector3(pointPosition.x, pointPosition.y, 0));
        }

        // Create a LineRenderer for connecting the last point to the first point
        GameObject connectingLineObject = new GameObject("ConnectingLineRenderer");
        connectingLineObject.transform.SetParent(panel.transform, false);
        LineRenderer connectingLineRenderer = connectingLineObject.AddComponent<LineRenderer>();
        connectingLineRenderer.material = lineMaterial;
        connectingLineRenderer.widthMultiplier = 3f;
        connectingLineRenderer.useWorldSpace = false; // Ensure it uses the panel's local space

        // Set the positions for the connecting line
        Vector3 firstPointPosition = pointsWithAngles[0].Point.GetComponent<RectTransform>().anchoredPosition;
        Vector3 lastPointPosition = pointsWithAngles[pointsWithAngles.Count - 1].Point.GetComponent<RectTransform>().anchoredPosition;
        connectingLineRenderer.positionCount = 2;
        connectingLineRenderer.SetPositions(new Vector3[] { firstPointPosition, lastPointPosition });

        // Add EdgeCollider component to the connectingLineObject
        EdgeCollider2D connectingEdgeCollider = connectingLineObject.AddComponent<EdgeCollider2D>();
        connectingEdgeCollider.points = new Vector2[] { firstPointPosition, lastPointPosition };

        // Add EdgeCollider component to the lineObject
        EdgeCollider2D edgeCollider = lineObject.AddComponent<EdgeCollider2D>();
        edgeCollider.points = pointsWithAngles.Select(point => point.Point.GetComponent<RectTransform>().anchoredPosition).ToArray();


        // Calculate perimeter and print to console
        float perimeter = CalculatePerimeter(pointsWithAngles);
        string perimeterMessage = perimeter.ToString();
        Debug.Log(perimeterMessage);
        if (textDisplay != null)
            textDisplay.text = perimeterMessage;
        Debug.Log("Perimeter: " + perimeter);

        // Calculate the average distance
        float averageDistance = CalculateAverageDistance(averagePointScreenPosition, projectedPoints);    
        Debug.Log("Average Distance: " + averageDistance);

        // Create centered lines
        CreateCenteredLines(panel, averagePointScreenPosition);

        // Destroy all intersection objects (excluding the average point)
        foreach (GameObject intersectionObject in intersectionObjects)
        {
            Destroy(intersectionObject);
        }

        // Destroy all projected points
        foreach (var point in projectedPoints)
        {
            Destroy(point);
        }

        PerformRaycasting(avgPointDraw);

        // Calculate and print distances between specific intersection points
        CalculateAndPrintIntersectionDistances();

        // Render the panel to an image and display it
        RenderPanelToImage();
    }

    // Function to calculate and print distances between specific intersection points
    void CalculateAndPrintIntersectionDistances()
    {
        GameObject intersectionPoint0 = GameObject.Find("IntersectionPoint0");
        GameObject intersectionPoint1 = GameObject.Find("IntersectionPoint1");
        GameObject intersectionPoint2 = GameObject.Find("IntersectionPoint2");
        GameObject intersectionPoint3 = GameObject.Find("IntersectionPoint3");
        GameObject intersectionPoint4 = GameObject.Find("IntersectionPoint4");
        GameObject intersectionPoint5 = GameObject.Find("IntersectionPoint5");
        GameObject intersectionPoint6 = GameObject.Find("IntersectionPoint6");
        GameObject intersectionPoint7 = GameObject.Find("IntersectionPoint7");

        if (intersectionPoint0 != null && intersectionPoint1 != null && intersectionPoint2 != null && intersectionPoint3 != null && intersectionPoint4 != null && intersectionPoint5 != null && intersectionPoint6 != null && intersectionPoint7 != null)
        {
            float distance01 = Vector2.Distance(intersectionPoint0.transform.position, intersectionPoint1.transform.position);
            float distance23 = Vector2.Distance(intersectionPoint2.transform.position, intersectionPoint3.transform.position);
            float distance47 = Vector2.Distance(intersectionPoint4.transform.position, intersectionPoint7.transform.position);
            float distance56 = Vector2.Distance(intersectionPoint5.transform.position, intersectionPoint6.transform.position);

            Debug.Log("Distance between IntersectionPoint0 and IntersectionPoint1: " + distance01);
            Debug.Log("Distance between IntersectionPoint2 and IntersectionPoint3: " + distance23);
            Debug.Log("Distance between IntersectionPoint4 and IntersectionPoint7: " + distance47);
            Debug.Log("Distance between IntersectionPoint5 and IntersectionPoint6: " + distance56);
            // Calculate difference between highest and lowest of distance47 and distance56
            float highestDistance = Mathf.Max(distance47, distance56);
            float lowestDistance = Mathf.Min(distance47, distance56);
            float difference = highestDistance - lowestDistance;
            float divisionResult = highestDistance != 0 ? difference / highestDistance : 0;

            // Round divisionResult to 3 decimal places
            divisionResult = Mathf.Round(divisionResult * 1000f) / 1000f;

            // Update TMP Text
            if (textDisplay != null)
            {
                string intersectionDistances = distance01 + "\n" + distance23 + "\n" + distance47 + "\n" + distance56 + "\n" + divisionResult + "%";
                    
                textDisplay.text += "\n" + intersectionDistances;
            }
        }
        else
        {
            Debug.LogWarning("One or more intersection points not found!");
        }
    }

    // Function to calculate the perimeter of the shape
    float CalculatePerimeter(List<PointWithAngle> pointsWithAngles)
    {
        float perimeter = 0f;
        Vector2 firstPoint = pointsWithAngles[0].Point.GetComponent<RectTransform>().anchoredPosition;
        for (int i = 1; i < pointsWithAngles.Count; i++)
        {
            Vector2 currentPoint = pointsWithAngles[i].Point.GetComponent<RectTransform>().anchoredPosition;
            Vector2 previousPoint = pointsWithAngles[i - 1].Point.GetComponent<RectTransform>().anchoredPosition;
            perimeter += Vector2.Distance(currentPoint, previousPoint);
        }
        // Add the distance between the last and first points to close the loop
        Vector2 lastPoint = pointsWithAngles[pointsWithAngles.Count - 1].Point.GetComponent<RectTransform>().anchoredPosition;
        perimeter += Vector2.Distance(lastPoint, firstPoint);
        return perimeter;
    }

    // Function to calculate the average distance
    float CalculateAverageDistance(Vector2 averagePointScreenPosition, List<GameObject> projectedPoints)
    {
        float totalDistance = 0f;
        foreach (GameObject point in projectedPoints)
        {
            Vector2 pointPosition = point.GetComponent<RectTransform>().anchoredPosition;
            totalDistance += Vector2.Distance(averagePointScreenPosition, pointPosition);
        }
        return totalDistance / projectedPoints.Count;
    }

    // Function to create centered lines
    void CreateCenteredLines(RectTransform panel, Vector2 averagePointScreenPosition)
    {
        // Create a new LineRenderer for the vertical line
        GameObject verticalLineObject = new GameObject("VerticalLineRenderer");
        verticalLineObject.transform.SetParent(panel.transform, false);
        LineRenderer verticalLineRenderer = verticalLineObject.AddComponent<LineRenderer>();
        verticalLineRenderer.material = lineMaterial;
        verticalLineRenderer.widthMultiplier = 3f;
        verticalLineRenderer.useWorldSpace = false; // Ensure it uses the panel's local space

        // Create a new LineRenderer for the horizontal line
        GameObject horizontalLineObject = new GameObject("HorizontalLineRenderer");
        horizontalLineObject.transform.SetParent(panel.transform, false);
        LineRenderer horizontalLineRenderer = horizontalLineObject.AddComponent<LineRenderer>();
        horizontalLineRenderer.material = lineMaterial;
        horizontalLineRenderer.widthMultiplier = 3f;
        horizontalLineRenderer.useWorldSpace = false; // Ensure it uses the panel's local space

        // Set the positions for the vertical line
        Vector3[] verticalLinePositions = new Vector3[2];
        verticalLinePositions[0] = new Vector3(averagePointScreenPosition.x, panel.rect.yMin + 10, 0);
        verticalLinePositions[1] = new Vector3(averagePointScreenPosition.x, panel.rect.yMax - 10, 0);
        verticalLineRenderer.positionCount = 2;
        verticalLineRenderer.SetPositions(verticalLinePositions);

        // Set the positions for the horizontal line
        Vector3[] horizontalLinePositions = new Vector3[2];
        horizontalLinePositions[0] = new Vector3(averagePointScreenPosition.x + panel.rect.xMin + 10, averagePointScreenPosition.y, 0);
        horizontalLinePositions[1] = new Vector3(averagePointScreenPosition.x + panel.rect.xMax - 10, averagePointScreenPosition.y, 0);
        horizontalLineRenderer.positionCount = 2;
        horizontalLineRenderer.SetPositions(horizontalLinePositions);

        // Create lines rotated by +15 and -15 degrees from the horizontal line
        CreateRotatedLine(panel, averagePointScreenPosition, horizontalLinePositions, 15f, "RotatedLine15");
        CreateRotatedLine(panel, averagePointScreenPosition, horizontalLinePositions, -15f, "RotatedLine-15");

    }

    // Helper function to create a rotated line
    void CreateRotatedLine(RectTransform panel, Vector2 center, Vector3[] baseLinePositions, float angle, string name)
    {
        GameObject rotatedLineObject = new GameObject(name);
        rotatedLineObject.transform.SetParent(panel.transform, false);
        LineRenderer rotatedLineRenderer = rotatedLineObject.AddComponent<LineRenderer>();
        rotatedLineRenderer.material = lineMaterial;
        rotatedLineRenderer.widthMultiplier = 3f;
        rotatedLineRenderer.useWorldSpace = false; // Ensure it uses the panel's local space

        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Vector3[] rotatedLinePositions = new Vector3[2];
        for (int i = 0; i < baseLinePositions.Length; i++)
        {
            float dx = baseLinePositions[i].x - center.x;
            float dy = baseLinePositions[i].y - center.y;
            float newX = dx * cos - dy * sin + center.x;
            float newY = dx * sin + dy * cos + center.y;
            rotatedLinePositions[i] = new Vector3(newX, newY, 0);
        }

        rotatedLineRenderer.positionCount = 2;
        rotatedLineRenderer.SetPositions(rotatedLinePositions);
    }

    void PerformRaycasting(GameObject avgPointDraw)
    {
        // Directions for raycasting
        Vector2[] directions = {
        Vector2.right, Vector2.left, Vector2.up, Vector2.down,
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 15f), Mathf.Sin(Mathf.Deg2Rad * 15f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 15f), -Mathf.Sin(Mathf.Deg2Rad * 15f)),
        new Vector2(-Mathf.Cos(Mathf.Deg2Rad * 15f), Mathf.Sin(Mathf.Deg2Rad * 15f)),
        new Vector2(-Mathf.Cos(Mathf.Deg2Rad * 15f), -Mathf.Sin(Mathf.Deg2Rad * 15f))
    };

        RaycastHit2D hit;
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 direction = directions[i];
            hit = Physics2D.Raycast(avgPointDraw.transform.position, direction);
            if (hit.collider != null)
            {
                // Check if the collider is an EdgeCollider2D
                EdgeCollider2D edgeCollider = hit.collider.GetComponent<EdgeCollider2D>();
                if (edgeCollider != null)
                {
                    // Instantiate point prefab at intersection point
                    GameObject point = Instantiate(pointPrefab, hit.point, Quaternion.identity);
                    point.name = "IntersectionPoint" + i; // Set a unique name
                }
            }
        }
    }

    // Function to render the panel to an image and display it
    void RenderPanelToImage()
    {
        // Create a RenderTexture with the size of the panel
        RenderTexture renderTexture = new RenderTexture((int)panel.rect.width, (int)panel.rect.height, 24);

        // Set the RenderTexture as the target of the Canvas
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        CanvasRenderer canvasRenderer = panel.GetComponent<CanvasRenderer>();
        canvasRenderer.SetMaterial(new Material(Shader.Find("UI/Default")), null);
        canvasRenderer.SetTexture(renderTexture);

        // Force a render of the Canvas
        canvas.enabled = false;
        canvas.enabled = true;

        // Create a new texture and read the render texture into it
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Convert the texture to a Sprite and assign it to the RawImage component
        imageDisplay.texture = texture;

        // Cleanup
        RenderTexture.active = null;
        Destroy(renderTexture);
    }
}