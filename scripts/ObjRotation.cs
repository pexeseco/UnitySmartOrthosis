using UnityEngine;

public class RotationScript : MonoBehaviour
{
    public Transform targetObject;
    public float rotationSpeed = 1.0f;

    private Vector3 lastMousePosition;

    private void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned in RotationScript!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            float rotationDelta = (currentMousePosition.x - lastMousePosition.x) * rotationSpeed;

            foreach (Transform child in targetObject)
            {
                Vector3 currentRotation = child.localEulerAngles;
                currentRotation.y += rotationDelta;
                child.localEulerAngles = currentRotation;
            }

            lastMousePosition = currentMousePosition;
        }
    }
}