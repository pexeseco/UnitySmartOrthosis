using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // Ensure the sprite always faces the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}