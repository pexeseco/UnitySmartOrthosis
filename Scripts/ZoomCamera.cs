using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float ScrollSpeed = 5.0f;
    [SerializeField] private float zoomOutMin = 3.5f;
    [SerializeField] private float zoomOutMax = 5.5f;

    private Camera ZoomCamera;

    private void Start()
    {
        ZoomCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float diff = currentMagnitude - prevMagnitude;

            TouchZoom(diff * 0.01f);
        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            MouseScrollZoom(scrollInput);
        }
    }

    void MouseScrollZoom(float scrollAmount)
    {
        if (ZoomCamera.orthographic)
        {
            ZoomCamera.orthographicSize -= scrollAmount * ScrollSpeed;
        }
        else
        {
            ZoomCamera.fieldOfView -= scrollAmount * ScrollSpeed;
        }
    }

    void TouchZoom(float increment)
    {
        float newOrthoSize = Mathf.Clamp(ZoomCamera.orthographicSize + increment, zoomOutMin, zoomOutMax);
        ZoomCamera.orthographicSize = newOrthoSize;
    }
}