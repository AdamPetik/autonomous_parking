using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// Decision requester for BaseAgent. Request decisions if the agent is activated.
/// </summary>
[RequireComponent(typeof(BaseAgent))]
public class CarAgentDecisionRequester : MonoBehaviour
{
    
    /// <summary>
    /// Period of how often to request a action.  
    /// </summary>
    [SerializeField]
    private int DecisionPeriod = 5;


    /// <summary>
    /// Whether to take actions between the decision period.  
    /// </summary>
    [SerializeField]
    private bool TakeActionsBetweenDecisions = true;

    private BaseAgent agent;

    private void Awake()
    {
        agent = GetComponent<BaseAgent>();

        Academy.Instance.AgentPreStep += MakeRequests;
    }

    private void OnDestroy()
    {
        if (Academy.IsInitialized)
        {
            Academy.Instance.AgentPreStep -= MakeRequests;
        }
    }   
    
    private void MakeRequests(int academyStepCount)
    {
        if (agent.IsActivated)
        {
            if (academyStepCount % DecisionPeriod == 0)
            {
                agent.RequestDecision();
            }
            if (TakeActionsBetweenDecisions)
            {
                agent.RequestAction();
            }
        }
    }
}
