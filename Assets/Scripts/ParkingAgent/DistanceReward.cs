using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Class that encapsulates distance reward calculation.  
/// </summary>
public class DistanceReward
{
    public enum DistanceRewardType
    {
        None,

        // distance_reward = distance_max_reward * 1 / clamp(d, 1, 100),
        // where d represents distance from the center of the nearest spot.
        Type1,

        // Same as Type1 but distance d is calculated from the parking spot trigger.
        Type1Trigger, 
        
        // reward = distance_max_reward if actual_distance < last_distance,
        // action_reward is added otherwise.
        Type2,

        // Same as Type2 but distance is calculated from the parking spot trigger.
        Type2Trigger

    }

    private ParkingAgent agent;
    private float actionReward = 0;
    protected float distanceMaxReward = 0f;

    protected DistanceRewardType distanceRewardType;
    private DistanceRewardType defaultDistanceRewardType;
    private float cachedDistance = 0f;

    public DistanceReward(ParkingAgent parkingAgent, DistanceRewardType defDistanceRewardType = DistanceRewardType.Type1)
    {
        agent = parkingAgent;
        defaultDistanceRewardType = defDistanceRewardType;
    }

    /// <summary>
    /// Initializes rewards values. 
    /// </summary>
    public void Initialize(EnvironmentParameters envParams, float actualActionReward = 0)
    {
        distanceRewardType = (DistanceRewardType) agent.EnvParameters.GetWithDefault("distance_reward_type", (float) defaultDistanceRewardType);
        distanceMaxReward = agent.EnvParameters.GetWithDefault("distance_max_reward", 0.001f);
        actionReward = actualActionReward;
    }

    /// <summary>
    /// Calculates and adds distance reward to the agent. 
    /// </summary>
    public void AddDistanceReward()
    {
        float distance = float.MaxValue;
        // adds reward based on reward type calculation
        switch (distanceRewardType)
        {
            case DistanceRewardType.Type1:
                distance = agent.SimManager.EnvManager.NearestPSpotManhattanDistance(agent.transform);
                AddType1DistanceReward(distance);
                break;

            case DistanceRewardType.Type1Trigger:
                distance = agent.SimManager.EnvManager.NearestPSpotTriggerManhattanDistance(agent.transform);
                AddType1DistanceReward(distance);
                break;
            
            case DistanceRewardType.Type2:
                distance = agent.SimManager.EnvManager.NearestPSpotManhattanDistance(agent.transform);
                AddType2DistanceReward(distance);
                break;

            case DistanceRewardType.Type2Trigger:
                distance = agent.SimManager.EnvManager.NearestPSpotTriggerManhattanDistance(agent.transform);
                AddType2DistanceReward(distance);
                break;
            
            case DistanceRewardType.None:
            default:
                // does nothing
                break;
        }
    }

    private void AddType1DistanceReward(float distance)
    {
        if (distanceMaxReward > 0f)
        {
            float bonusRate = Mathf.Clamp(distance, 1f, 100f);
            float distanceReward = distanceMaxReward * 1f / bonusRate;
            agent.AddReward(distanceReward);
        }
    }

    private void AddType2DistanceReward(float distance)
    {
        if (distanceMaxReward > -1f)
        {
            if (distance < cachedDistance || distance <= 1)
            {
                agent.AddReward(distanceMaxReward);
            } 
            cachedDistance = distance;
        }
    }

    /// <summary>
    /// Updates cached distance which is needed for Type2 calculation.
    /// </summary>
    public void UpdateCachedDistance()
    {
        switch (distanceRewardType)
        {
            case DistanceRewardType.Type2:
                cachedDistance = agent.SimManager.EnvManager.NearestPSpotManhattanDistance(agent.transform);
                break;

            case DistanceRewardType.Type2Trigger:
                cachedDistance = agent.SimManager.EnvManager.NearestPSpotTriggerManhattanDistance(agent.transform);
                break;

            default:
                // does nothing, in other cases cachedDistance is not needed
                break;
        }
    }
}