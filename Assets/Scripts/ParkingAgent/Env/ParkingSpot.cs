using UnityEngine;

/// <summary>
/// Component which represents parking spot.
/// </summary>
public class ParkingSpot : MonoBehaviour, IColliderObserver
{
    [SerializeField]
    private Transform trigger;

    [SerializeField]
    private GameObject staticCarPrefab;

    /// <summary>
    /// If it is true, agent may park in this spot otherwise it is occupied by
    /// static car to prevent agent to park at this spot. This feature allows
    /// us to create parking lot, where we prevent to create two or more free
    /// parking spots next to each other.
    /// </summary>
    [SerializeField]
    private bool isParkable = true;

    public bool IsParkable {get {return isParkable;} }

    private void Awake()
    {
        if (!isParkable)
        {
            // if it is not "parkable" spot ar is instantiated in this parking spot
            InstantiateStaticCar();
            DisableTrigger();
        }
        else
        {
            // otherwise ColliderObservavble is added to the trigger component to listen to collisions.
            ColliderObservable observable = trigger.gameObject.AddComponent<ColliderObservable>();
            observable.Initialize(this);
        }
    }

    public Transform GetTriggerTransform()
    {
        return trigger;
    }

    public void DisableTrigger()
    {
        trigger.GetComponent<BoxCollider>().enabled = false;
    }

    public void EnableTrigger()
    {
        if (isParkable) trigger.GetComponent<BoxCollider>().enabled = true;
    }

    public bool Enabled {get {return trigger.GetComponent<BoxCollider>().enabled;} }

    /// <summary>
    /// Instantiates static car in this parking spot
    /// </summary>
    private void InstantiateStaticCar ()
    {
        Object.Instantiate(staticCarPrefab, transform.position, transform.rotation);
    }

    public void OnCollisionEnterListener(Collision collision)
    {
        DisableByTag(collision.gameObject.tag);
    }

    public void OnTriggerEnterListener(Collider other)
    {
        DisableByTag(other.gameObject.tag);
    }

    /// <summary>
    /// Disables trigger if the tag is equal to "Agent".
    /// </summary>
    private void DisableByTag(string tag)
    {
        if (tag == "Agent")
        {
            DisableTrigger();
        }
    }
}
