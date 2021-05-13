using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component for positiong object inside plane on z coordinate.
/// </summary>
public class AreaPositioner : MonoBehaviour
{
    /// <summary>
    /// Positions the object inside the plane on z coordinate.
    /// </summary>
    /// <param name="obj"> The object to reposition. </param>
    /// <param name="distanceFromObjects"> List of objects from which the minimal distance is defined by distance param. </param>
    /// <param name="distance"> Minimal distance from all objects from distanceFromObjects.</param>
    /// <param name="rotationOffset"> Y rotation of obj. </param> 
    public void Reposition(GameObject obj, List<GameObject> distanceFromObjects, float distance, float rotationOffset)
    {
        float minDistance = float.MaxValue;
        Vector3 initPosition;

        // finds random position on z coordinate, which distance from all objects from distanceFromObjects list,
        // is larger than distace parameter
        do
        {
            minDistance = float.MaxValue;
            float randomZ = Random.Range(transform.localScale.z*-5, transform.localScale.z*5);
            initPosition = transform.position + transform.forward * randomZ;
            
            foreach(GameObject distanceObject in distanceFromObjects)
            {
                float tmpDistance = Vector3.Distance(initPosition, distanceObject.transform.position);
                if (tmpDistance < minDistance) minDistance = tmpDistance;
            }
        } 
        while (minDistance <= distance);

        // applyes rotation offset and position to the object
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        eulerAngles.y += rotationOffset;

        obj.transform.rotation = Quaternion.Euler(eulerAngles);
        obj.transform.position = initPosition;
    }
}
