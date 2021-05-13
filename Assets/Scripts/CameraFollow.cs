using UnityEngine;

/// <summary>
/// Component used by camera to follow the target.  
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;
    public float translationSpeed;
    public float rotationSpeed;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();    
    }

    private void HandleTranslation()
    {
        Vector3 targetPosition = target.TransformPoint(offset);
         // vymen za smoothdown ak bude treba
        transform.position = Vector3.Lerp(transform.position, targetPosition, translationSpeed * Time.deltaTime);

    }

    private void HandleRotation()
    {
        Vector3 directon = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(directon, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}
