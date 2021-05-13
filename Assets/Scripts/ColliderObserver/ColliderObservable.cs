using UnityEngine;


/// <summary>
/// Observable componet wich report OnCollisonEnter and OnTriggerEnter
/// events to the observer.
/// </summary>
public class ColliderObservable : MonoBehaviour
{
    private IColliderObserver listener;
    public void Initialize(IColliderObserver observer)
    {
        listener = observer;
    }
    void OnCollisionEnter(Collision collision)
    {
        listener.OnCollisionEnterListener(collision);
    }
    void OnTriggerEnter(Collider other)
    {
        listener.OnTriggerEnterListener(other);
    }
}