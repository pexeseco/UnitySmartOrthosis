using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRenderTextureExample : MonoBehaviour
{
    public Camera cameraToRender;
    public Material materialToAssign;

    void Start()
    {
        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        cameraToRender.targetTexture = renderTexture;
        materialToAssign.mainTexture = renderTexture;
    }
}