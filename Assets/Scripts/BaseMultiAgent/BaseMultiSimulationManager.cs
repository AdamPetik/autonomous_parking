using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;


/// <summary>
/// Base implementation of Simulation manager. Supports one or more agents
/// in one learning environment.
/// </summary>
public abstract class BaseMultiSimulationManager<A> : MonoBehaviour where A : BaseAgent
{   
    /// <summary>
    /// List of all agents in the learning evironment.
    /// </summary>
    private List<A> agents;
    
    
    /// <summary>
    /// Temporal stack of all initialized agents in learning environment.
    /// </summary>
    private List<A> initializedAgents;
    
    /// <summary>
    /// Temporal stack of all ended agents in learning environment.
    /// </summary>
    private List<A> endedAgents;

    public BaseMultiSimulationManager()
    {
        initializedAgents = new List<A>();
        endedAgents = new List<A>();
    }

    /// <summary>
    /// Collects agents found in children objects
    /// </summary>
    public void CollectAgents()
    {
        agents = new List<A>(gameObject.GetComponentsInChildren<A>());
    }

    /// <summary>
    /// Runs initialization (calls OnAgentInitialized method) if all agents
    /// are ready for initialization.
    /// </summary>
    public virtual void Initialize(A agent, EnvironmentParameters envParams)
    {
        Debug.LogFormat("Agent {0} is ready for initialization.", agent.gameObject.name);
        initializedAgents.Add(agent);
        
        if (agents == null)
        {
            CollectAgents();
        }

        // only if initialized agents count is equal to all agents the initialization
        // beggins. 
        if (initializedAgents.Count == agents.Count)
        {
            Debug.Log("Initializing agents started.");

            OnAgentsInitialized(initializedAgents, envParams);

            initializedAgents.Clear();

            Debug.Log("Initializing agents ended.");

        }
    }

    /// <summary>
    /// Called when all the agents are ready to initialize.
    /// </summary>
    protected abstract void OnAgentsInitialized(List<A> agents, EnvironmentParameters envParams);

    /// <summary>
    /// Runs ending routine (calls the method OnAgentsEnd) if all the agents want to end episode.
    /// </summary>
    /// <param name="agent"> Agent that want to end episode. </param>
    public virtual void AgentEnds(A agent)
    {
        Debug.LogFormat("Agent {0} ends.", agent.gameObject.name);
        
        // have to deactivate an agent to stop making actions
        agent.Deactivate();
        endedAgents.Add(agent);

        // only if ended agents count is equal to all agents the endinf routine
        // beggins. 
        if (endedAgents.Count == agents.Count)
        {
            Debug.Log("Ending epizod for agents.");
            
            OnAgentsEnd(endedAgents);

            endedAgents.Clear();

            Debug.Log("Ending epizod for agents ended.");
        }
    }

    /// <summary>
    /// Called when all the agents are want to end episode.
    /// </summary>
    protected abstract void OnAgentsEnd(List<A> agents);

    /// <summary>
    /// Resets ended agents and initialized agents buffer lists and ends
    /// epizode for each agent.
    /// </summary>
    public virtual void Reset()
    {
        endedAgents.Clear();
        initializedAgents.Clear();
        agents.ForEach(agent => AgentEnds(agent));
    }
}