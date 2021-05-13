using UnityEngine;

public class Utils {
    public static int RandomSign()
    {
        return Random.value < .5 ? 1 : -1;
    }

    public static float ManhattanDistance(Transform contextTransform, Transform transform1, Transform transform2)
    {
        if(contextTransform == null) { throw new System.ArgumentNullException(); }
        if(transform1 == null) { throw new System.ArgumentNullException(); }
        if(transform2 == null) { throw new System.ArgumentNullException(); }
        
        Vector3 localPos1 = contextTransform.InverseTransformPoint(transform1.position);
        Vector3 localPos2 = contextTransform.InverseTransformPoint(transform2.position);
        Vector3 diff = (localPos1 - localPos2);
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.z);
    }

    public static float ManhattanDistanceFromVectors(Vector3 position1, Vector3 position2)
    {
        if(position1 == null) { throw new System.ArgumentNullException(); }
        if(position2 == null) { throw new System.ArgumentNullException(); }
        
        Vector3 diff = (position1 - position2);
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.z);
    }

    public static float YRotationDiff(Transform transform1, Transform transform2)
    {
        if(transform1 == null) { throw new System.ArgumentNullException(); }
        if(transform2 == null) { throw new System.ArgumentNullException(); }
        
        float rotation1 = Mathf.Abs(transform1.eulerAngles.y % 180);
        float rotation2 = Mathf.Abs(transform2.eulerAngles.y % 180);
        float rotation = Mathf.Abs(rotation1 - rotation2);
        
        return rotation;
    }

    public static float XPositionDiff(Transform transform1, Transform transform2)
    {
        if(transform1 == null) { throw new System.ArgumentNullException(); }
        if(transform2 == null) { throw new System.ArgumentNullException(); }
        
        Vector3 inversed1 = transform2.InverseTransformDirection(transform1.position);
        Vector3 inversed2 = transform2.InverseTransformDirection(transform2.position);

        float distance = Mathf.Abs(inversed1.x - inversed2.x);
        
        return distance;
    }
}