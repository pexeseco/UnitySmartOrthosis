using UnityEngine;

[CreateAssetMenu(fileName = "ObjImportSettings", menuName = "ScriptableObjects/ObjImportSettings", order = 1)]
public class ObjImportSettings : ScriptableObject
{
    public bool readWriteEnabled = true;
    public bool generateColliders = true;
}