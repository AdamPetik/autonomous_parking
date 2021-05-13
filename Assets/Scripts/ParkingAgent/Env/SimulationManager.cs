using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// Simulation manager for parking agents.
/// </summary>
[RequireComponent(typeof(EnvManager))]
public class SimulationManager : BaseMultiSimulationManager<ParkingAgent>
{
    private EnvManager envManager;

    public EnvManager EnvManager { get {return envManager;} }

    private void Awake()
    {
        envManager = GetComponent<EnvManager>();
    }
    
    protected override void OnAgentsEnd(List<ParkingAgent> agents)
    {
        envManager.AgentsEnd(agents);

        // at the end, ends episode for all agents
        agents.ForEach(endedAgent => endedAgent.EndAgent());
    }

    protected override void OnAgentsInitialized(List<ParkingAgent> agents, EnvironmentParameters envParams)
    {
        envManager.InitializeAgents(agents, envParams);

        // after environment initiaization, activates the agents.
        agents.ForEach(agent => agent.Activate());
    }
}
