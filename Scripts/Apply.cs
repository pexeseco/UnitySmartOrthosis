using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apply : MonoBehaviour
{
    public GameObject importedObject;

    public GameObject parentObject;

    void Start()
    {
        //UpdateOBJ.UpdateCenter(importedObject, parentObject);
    }

    public void Execute()
    {
        UpdateOBJ_v2.UpdateImportedObject(importedObject);
    }

}
