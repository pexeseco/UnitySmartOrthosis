//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;

//public class Screenshot : MonoBehaviour
//{
//    public GameObject objectToCapture; // Reference to the object you want to capture

//    public void TakeScreenshot()
//    {
//        // Check if objectToCapture is assigned
//        if (objectToCapture == null)
//        {
//            Debug.LogError("Please assign the object to capture!");
//            return;
//        }

//        // Capture screenshot
//        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
//        Camera camera = objectToCapture.GetComponent<Camera>();
//        if (camera == null)
//        {
//            Debug.LogError("The assigned object doesn't have a camera component!");
//            return;
//        }

//        camera.targetTexture = renderTexture;
//        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
//        camera.Render();
//        RenderTexture.active = renderTexture;
//        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
//        camera.targetTexture = null;
//        RenderTexture.active = null;
//        Destroy(renderTexture);

//        // Save screenshot
//        byte[] bytes = screenshot.EncodeToPNG();
//        string filename = "screenshot_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
//        File.WriteAllBytes(Application.persistentDataPath + "/" + filename, bytes);
//        Debug.Log("Screenshot saved to: " + Application.persistentDataPath + "/" + filename);
//    }
//}