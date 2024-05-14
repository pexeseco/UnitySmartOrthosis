using UnityEngine;

public class IntersectionFinder : MonoBehaviour
{
    // Reference to the parent object of the target object
    public Transform targetParent;

    void Start()
    {
        FindIntersection();
    }

    void FindIntersection()
    {
        // Check if the target parent object exists
        if (targetParent != null)
        {
            // Find the mmGroup0 object among the grandchildren of the target parent
            Transform mmGroup0 = FindMMGroup0(targetParent);

            // Check if the mmGroup0 object exists
            if (mmGroup0 != null)
            {
                // Check for intersection between this object's collider and the mmGroup0 object's collider
                Collider thisCollider = GetComponent<Collider>();
                Collider mmGroup0Collider = mmGroup0.GetComponent<Collider>();

                if (thisCollider != null && mmGroup0Collider != null)
                {
                    if (thisCollider.bounds.Intersects(mmGroup0Collider.bounds))
                    {
                        Debug.Log("Intersection found with the mmGroup0 object!");
                        // Perform actions you want when intersection is found
                    }
                    else
                    {
                        Debug.Log("Intersection not found with the mmGroup0 object.");
                    }
                }
                else
                {
                    Debug.LogError("Collider not found on either this object or the mmGroup0 object.");
                }
            }
            else
            {
                Debug.LogError("mmGroup0 object not found among the grandchildren of the target parent object.");
            }
        }
        else
        {
            Debug.LogError("Target parent object not assigned.");
        }
    }

    Transform FindMMGroup0(Transform parent)
    {
        // Iterate through all children of the parent
        foreach (Transform child in parent)
        {
            // Iterate through all children of the current child
            foreach (Transform grandchild in child)
            {
                // Check if the grandchild's name matches "mmGroup0"
                if (grandchild.name == "mmGroup0")
                {
                    // Return the grandchild if it matches
                    return grandchild;
                }
            }
        }
        // Return null if "mmGroup0" grandchild is not found
        return null;
    }
}