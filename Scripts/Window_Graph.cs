using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using System.Linq;

public class Window_Graph : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Text coordinatesText;
    [SerializeField] private TMP_Dropdown dropdownMenu;
    [SerializeField] private TMP_InputField inputFileInputField; // Input field for the CSV file name
    [SerializeField] private Button loadButton; // Button to trigger loading the CSV file
    [SerializeField] private TMP_Dropdown planeDropdown; // Dropdown for selecting the plane
    private RectTransform graphContainer;
    private GameObject highlightedCircle;
    private Dictionary<GameObject, (float x, float y)> circlePositions = new Dictionary<GameObject, (float x, float y)>();
    private List<List<int>> dataList = new List<List<int>>();
    private List<string> columnOptions = new List<string>();
    private Dictionary<string, List<List<int>>> planeDataMap = new Dictionary<string, List<List<int>>>();
    private List<string> planeOptions = new List<string>();

    private void Awake()
    {
        coordinatesText = GetComponentInChildren<Text>();
        if (coordinatesText == null)
        {
            Debug.LogError("Coordinates Text not found.");
        }
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();

        // Assign the button click event
        loadButton.onClick.AddListener(OnLoadButtonClick);

        // Initialize the dropdowns but don't display the graph yet
        InitializeDropdown();
    }

    private void InitializeDropdown()
    {
        dropdownMenu.ClearOptions();
        columnOptions.Clear();
        planeDropdown.ClearOptions();
        planeOptions.Clear();
    }

    private void LoadDataFromCSV(string fileName)
    {
        string filePath = Application.dataPath + "/Data/" + fileName + ".csv";
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        dataList.Clear();
        planeDataMap.Clear();
        using (StreamReader reader = new StreamReader(filePath))
        {
            if (!reader.EndOfStream)
            {
                string[] columnHeaders = reader.ReadLine().Split(',');
                for (int i = 1; i < columnHeaders.Length - 1; i++) // Adjust to ignore the last column (plane)
                {
                    columnOptions.Add(columnHeaders[i]);
                }

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    List<int> row = new List<int>();
                    if (values.Length > 1) // Ensure there are values in the row
                    {
                        for (int i = 1; i < values.Length - 1; i++) // Adjust to ignore the last column (plane)
                        {
                            if (int.TryParse(values[i], out int intValue))
                            {
                                row.Add(intValue);
                            }
                        }

                        string plane = values[values.Length - 1];
                        if (!planeDataMap.ContainsKey(plane))
                        {
                            planeDataMap[plane] = new List<List<int>>();
                            planeOptions.Add(plane);
                        }
                        planeDataMap[plane].Add(row);
                    }
                }
            }
        }

        dropdownMenu.AddOptions(columnOptions);
        dropdownMenu.onValueChanged.AddListener(ChangeGraph);
        
        planeDropdown.AddOptions(planeOptions);
        planeDropdown.onValueChanged.AddListener(delegate { ShowGraph(); });

        // Display the graph initially for the first plane
        ShowGraph();
    }

    private void ShowGraph()
    {
        ResetGraph();

        if (planeOptions.Count == 0 || planeDropdown.value >= planeOptions.Count)
        {
            Debug.LogError("No planes available to display.");
            return;
        }

        string selectedPlane = planeOptions[planeDropdown.value];
        if (!planeDataMap.ContainsKey(selectedPlane))
        {
            Debug.LogError("Selected plane data not found.");
            return;
        }

        List<List<int>> selectedPlaneData = planeDataMap[selectedPlane];
        if (selectedPlaneData.Count == 0)
        {
            Debug.LogError("No data to display for the selected plane.");
            return;
        }

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        // Find the maximum y-value in the dataset
        int maxYValue = selectedPlaneData.Max(row => row[0]);

        // Calculate the distance between each x-value
        float xDistance = graphWidth / (selectedPlaneData.Count - 1);

        GameObject lastCircleGameObject = null;

        // Iterate over the selectedPlaneData to position the points
        for (int i = 0; i < selectedPlaneData.Count; i++)
        {
            float xPosition = i * xDistance;
            float yPosition = (selectedPlaneData[i][0] / (float)maxYValue) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), i);
            circlePositions.Add(circleGameObject, (xPosition, selectedPlaneData[i][0])); // Store position and corresponding value

            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }

            lastCircleGameObject = circleGameObject;
        }

        // Add additional points for CR or CVAI
        if (dropdownMenu.options[dropdownMenu.value].text == "CR" || dropdownMenu.options[dropdownMenu.value].text == "CVAI")
        {
            float leftX = 0; // Leftmost x coordinate
            float rightX = graphWidth; // Rightmost x coordinate

            float yCR = dropdownMenu.options[dropdownMenu.value].text == "CR" ? 0.78f : 3.5f; // Determine y value based on dropdown selection

            // Create left point
            GameObject leftCircle = CreateCircle(new Vector2(leftX, yCR * graphHeight), -1);
            circlePositions.Add(leftCircle, (leftX, yCR));

            // Create right point
            GameObject rightCircle = CreateCircle(new Vector2(rightX, yCR * graphHeight), -2);
            circlePositions.Add(rightCircle, (rightX, yCR));

            // Connect left and right points to the first and last data points respectively
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, leftCircle.GetComponent<RectTransform>().anchoredPosition);
                CreateDotConnection(new Vector2(circlePositions.Values.First().Item1, 0), rightCircle.GetComponent<RectTransform>().anchoredPosition);
            }
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition, int index)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image), typeof(EventTrigger)); // Add EventTrigger component
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        // Add event triggers
        EventTrigger trigger = gameObject.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        trigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
        trigger.triggers.Add(entry);

        highlightedCircle = gameObject;

        return gameObject;
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 2f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Implement interface methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject hoveredCircle = eventData.pointerEnter.gameObject;
        hoveredCircle.GetComponent<Image>().color = Color.gray; // Change the color of the hovered circle

        if (coordinatesText != null && circlePositions.ContainsKey(hoveredCircle))
        {
            var positionAndValue = circlePositions[hoveredCircle];
            coordinatesText.text = $"X={positionAndValue.x} Y={positionAndValue.y}";
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject exitedCircle = eventData.pointerEnter.gameObject;
        exitedCircle.GetComponent<Image>().color = Color.white;

        if (coordinatesText != null)
        {
            coordinatesText.text = "";
        }
    }

    // Method to change the graph data
    public void ChangeGraph(int newIndex)
    {
        // No need to reload the CSV file here, just show the graph for the selected column
        ShowGraph();
    }

    // Method to reset the graph
    private void ResetGraph()
    {
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }
        circlePositions.Clear();
    }

    // Method to handle button click
    private void OnLoadButtonClick()
    {
        string fileName = inputFileInputField.text;
        if (!string.IsNullOrEmpty(fileName))
        {
            LoadDataFromCSV(fileName);
            ResetGraph();
            ShowGraph();
        }
    }
}