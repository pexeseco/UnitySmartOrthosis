using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class IntersectionProjection : MonoBehaviour
{
    public RectTransform panel; // Reference to the UI panel where the 2D plane will be represented
    public GameObject pointPrefab; // Prefab of the UI element representing a projected point
    public Material lineMaterial; // Material for the line renderer
    public Material circleMaterial; // Material for the circle renderer
    public RawImage imageDisplay; // RawImage UI element to display the rendered texture

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
            point.name = intersectionObject.name;

            // Set the position of the UI element within the panel
            RectTransform pointTransform = point.GetComponent<RectTransform>();
            pointTransform.anchoredPosition = screenPoint + panel.sizeDelta / 2f; // Adjust position relative to panel center

            projectedPoints.Add(point);
        }

        // Create a new UI element (point) representing the average point
        GameObject avgPoint = Instantiate(pointPrefab, panel);
        avgPoint.name = averagePointObject.name; // Set the name
        RectTransform avgPointTransform = avgPoint.GetComponent<RectTransform>();
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
        lineRenderer.positionCount = pointsWithAngles.Count; // Exclude the average point

        // Set the positions of the LineRenderer for the existing line (excluding the average point)
        for (int i = 0; i < pointsWithAngles.Count; i++)
        {
            Vector2 pointPosition = pointsWithAngles[i].Point.GetComponent<RectTransform>().anchoredPosition;
            lineRenderer.SetPosition(i, new Vector3(pointPosition.x, pointPosition.y, 0));
        }

        // Calculate perimeter and print to console
        float perimeter = CalculatePerimeter(pointsWithAngles);
        Debug.Log("Perimeter: " + perimeter);

        // Calculate the average distance
        float averageDistance = CalculateAverageDistance(averagePointScreenPosition, projectedPoints);
        Debug.Log("Average Distance: " + averageDistance);

        // Draw the circle with the average distance
        DrawCircle(averagePointScreenPosition, averageDistance, 100, Color.red);

        // Create centered lines
        CreateCenteredLines(panel, averagePointScreenPosition);

        // Destroy all intersection objects (excluding the average point)
        foreach (GameObject intersectionObject in intersectionObjects)
        {
            Destroy(intersectionObject);
        }

        // Destroy the average point object
        Destroy(averagePointObject);

        // Destroy all projected points
        foreach (var point in projectedPoints)
        {
            Destroy(point);
        }

        // Render the panel to an image and display it
        RenderPanelToImage();
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

    // Function to draw a circle
    void DrawCircle(Vector2 center, float radius, int segments, Color color)
    {
        GameObject circleObject = new GameObject("CircleRenderer");
        circleObject.transform.SetParent(panel.transform, false);
        LineRenderer circleRenderer = circleObject.AddComponent<LineRenderer>();
        circleRenderer.material = new Material(circleMaterial); // Use a different material for the circle
        circleRenderer.material.color = color; // Set the circle color
        circleRenderer.widthMultiplier = 3f;
        circleRenderer.useWorldSpace = false;

        circleRenderer.positionCount = segments + 1;
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(angle) * radius + center.x;
            float y = Mathf.Sin(angle) * radius + center.y;
            circleRenderer.SetPosition(i, new Vector3(x, y, 0));
            angle += 2f * Mathf.PI / segments;
        }
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