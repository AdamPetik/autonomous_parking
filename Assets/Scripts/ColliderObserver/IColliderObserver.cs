using UnityEngine;

/// <summary>
/// Observer interface for observing collision.
/// </summary>
public interface IColliderObserver
{
    void OnCollisionEnterListener(Collision collision);
    void OnTriggerEnterListener(Collider other);
}