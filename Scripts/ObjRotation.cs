using UnityEngine;
using UnityEngine.UI;

public class RotationScript : MonoBehaviour
{
    public float rotationSpeed = 1.0f;
    private Transform targetObject;
    private Vector3 lastMousePosition;

    private void Update()
    {
        // Find the object named "Object"
        GameObject objectToRotate = GameObject.Find("Object");

        // Check if the object exists
        if (objectToRotate == null)
        {
            Debug.LogWarning("Object named 'Object' not found in the scene.");
            return;
        }

        // Assign the object's transform as the target object
        targetObject = objectToRotate.transform;

        // Check if the target object is assigned
        if (targetObject == null)
        {
            Debug.LogWarning("Target object is not assigned.");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            float rotationDelta = (currentMousePosition.x - lastMousePosition.x) * rotationSpeed;

            // Rotate the object around its Y-axis
            targetObject.Rotate(Vector3.up, rotationDelta);

            lastMousePosition = currentMousePosition;
        }
    }

    // Function to reset the rotation to specified values
    public void ResetRotation()
    {
        if (targetObject != null)
        {
            targetObject.rotation = Quaternion.Euler(new Vector3(0f, -180f, 0f));
        }
        
    }

}