using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parking agent observer which listens to the following events:
///     - max steps reached -> agent calls OnMaxStepsReached method
///     - agent successfuly parked -> agent calls OnParked method
///     - agent collided -> agent calls OnCollision method
/// </summary>
public abstract class ParkingAgentObserver : MonoBehaviour
{
    private List<ParkingAgent> agents;

    public List<ParkingAgent> ObservableAgents {get {return agents;} }
    private void Start()
    {
        agents = new List<ParkingAgent>();

        // primary tries to register ParkingAgent component on the same object only if exists.
        ParkingAgent agent = GetComponent<ParkingAgent>();
        if (agent == null)
        {
            // otherwise registers to all agents in children game objects
            agents = new List<ParkingAgent>(GetComponentsInChildren<ParkingAgent>());
        } 
        else
        {
            agents.Add(agent);
        }
        Register();
    }

    private void Register()
    {
        agents?.ForEach(agent => agent?.RegisterObserver(this));
    }

    private void Unregister()
    {
        agents?.ForEach(agent => agent?.UnregisterObserver(this));
    }

    private void OnDestroy()
    {
        Unregister();
    }

    /// <summary>
    /// Called if max steps are reached. 
    /// </summary>
    /// <param name="agent"> Agent who has reached the max steps.</param>
    public abstract void OnMaxStepsReached(ParkingAgent agent);
    
    /// <summary>
    /// Called if agent successfuly parked. 
    /// </summary>
    /// <param name="collider"> Collider of parking spot. </param>
    /// <param name="agent"> Agent who has successfuly parked.</param>
    public abstract void OnParked(Collider collider, ParkingAgent agent);

    /// <summary>
    /// Called if agent collided. 
    /// </summary>
    /// <param name="collider"> Collider. </param>
    /// <param name="agent"> Agent who has collided</param>
    public abstract void OnCollision(Collider collider, ParkingAgent agent);
}