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
    [SerializeField] private TMP_InputField inputFileInputField; // New input field for the CSV file name
    private RectTransform graphContainer;
    private GameObject highlightedCircle;
    private Dictionary<GameObject, (float x, float y)> circlePositions = new Dictionary<GameObject, (float x, float y)>();
    private List<List<int>> dataList = new List<List<int>>();
    private List<string> columnOptions = new List<string>();

    private void Awake()
    {
        coordinatesText = GetComponentInChildren<Text>();
        if (coordinatesText == null)
        {
            Debug.LogError("Coordinates Text not found.");
        }
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        coordinatesText = GetComponentInChildren<Text>(); // Assuming the text is a child of the graph object

        InitializeDropdown();

        // Display the graph
        ShowGraph();
    }

    private void InitializeDropdown()
    {
        dropdownMenu.ClearOptions();
        columnOptions.Clear();

        string filePath = Application.dataPath + "/0.csv"; // Assuming 0.csv exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            if (!reader.EndOfStream)
            {
                string[] columnHeaders = reader.ReadLine().Split(',');
                for (int i = 1; i < columnHeaders.Length; i++) // Start from index 1 to ignore the first column
                {
                    columnOptions.Add(columnHeaders[i]);
                }
            }
        }

        dropdownMenu.AddOptions(columnOptions);
        dropdownMenu.onValueChanged.AddListener(ChangeGraph);
    }

    private void LoadDataFromCSV(string fileName)
    {
        string filePath = Application.dataPath + "/" + fileName + ".csv";
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        dataList.Clear();
        using (StreamReader reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');
                List<int> row = new List<int>();
                if (values.Length > 1) // Ensure there are values in the row
                {
                    for (int i = 1; i < values.Length; i++) // Start from index 1 to ignore the first column
                    {
                        if (int.TryParse(values[i], out int intValue))
                        {
                            row.Add(intValue);
                        }
                    }
                    dataList.Add(row);
                }
            }
        }
    }

    private void ShowGraph()
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        if (dataList.Count == 0)
        {
            Debug.LogError("No data to display.");
            return;
        }

        // Find the maximum y-value in the dataset
        int maxYValueIndex = FindMaxYValueIndex();
        int maxYValue = dataList[maxYValueIndex][0];

        // Calculate the distance between each x-value
        float xDistance = graphWidth / (dataList.Count - 1);

        GameObject lastCircleGameObject = null;

        // Iterate over the dataList to position the points
        for (int i = 0; i < dataList.Count; i++)
        {
            float xPosition = i * xDistance;
            float yPosition = (dataList[i][0] / (float)maxYValue) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), i);
            circlePositions.Add(circleGameObject, (xPosition, dataList[i][0])); // Store position and corresponding value

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

    private int FindMaxYValueIndex()
    {
        int maxYValueIndex = 0;
        int maxYValue = dataList[0][0];
        for (int i = 1; i < dataList.Count; i++)
        {
            if (dataList[i][0] > maxYValue)
            {
                maxYValue = dataList[i][0];
                maxYValueIndex = i;
            }
        }
        return maxYValueIndex;
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
        LoadDataFromCSV(columnOptions[newIndex]);
        ResetGraph();
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

    // Method to handle input field text change
    public void OnInputFieldTextChanged(string text)
    {
        if (text != "")
        {
            LoadDataFromCSV(text);
            ResetGraph();
            ShowGraph();
        }
    }
}