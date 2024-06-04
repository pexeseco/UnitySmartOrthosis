using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IntersectionDrawer : MonoBehaviour
{
    public GameObject prefab;
    private int index = 0;

    public void DrawIntersection(Vector3 v3, Quaternion q)
    {
        GameObject intersectionObject = Instantiate(prefab, v3, q);
        intersectionObject.name = "IntersectionObject" + (index + 1);
        intersectionObject.tag = "IntersectionObject";
        index++;
    }
}