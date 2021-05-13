using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// Base agent classed used in project. 
/// </summary>
abstract public class BaseAgent : Agent
{
    
    /// <summary>
    /// Contains environment parameters from config file.
    /// </summary>
    protected EnvironmentParameters environmentParameters;
    public EnvironmentParameters EnvParameters { get { return environmentParameters; } }

    /// <summary>
    /// Indicates whether the agent was interrupted by reaching max steps.
    /// </summary>
    private bool interrupted = false;
    public bool Interrupted { get {return interrupted;} }

    /// <summary>
    /// Indicates whether the agent is activated or not. 
    /// </summary>
    private bool isActivated = false;
    public bool IsActivated { get {return isActivated;} }

    public override void Initialize()
    {
        base.Initialize();
        // registers PreStep() and MaxStepsReachedCheck() methods, which will
        // be called before every agent step.
        Academy.Instance.AgentPreStep += PreStep;
        Academy.Instance.AgentPreStep += MaxStepReachedCheck;
    }

    private void OnDestroy()
    {
        if (Academy.IsInitialized)
        {
            Academy.Instance.AgentPreStep -= PreStep;
            Academy.Instance.AgentPreStep -= MaxStepReachedCheck;
        }
    }

    public override void OnEpisodeBegin()
    {
        // loads environment parameter from config file at the beginning of every episode.
        environmentParameters = Academy.Instance.EnvironmentParameters;
    }

    public virtual void Activate()
    {
        isActivated = true;
    }

    public virtual void Deactivate()
    {
        isActivated = false;
    }

    /// <summary>
    /// Method which are called before every agent step.
    /// </summary>
    protected virtual void PreStep(int academyStepCount) {}
    
    /// <summary>
    /// Method checks whether max steps was reached. If max steps was reached,
    /// calls the OnMaxStepReached().
    /// </summary>
    private void MaxStepReachedCheck(int academyStepCount)
    {
        // Checking step count with value MaxStep - 2, so that we can end episode in time.
        // Otherwise, the episode would be terminated by Academy and that would disrupt
        // learning process of multiple agents in one learning environment.
        if ((StepCount >= MaxStep - 2) && (MaxStep > 0))
        {
            // if multiple agents are in one environment, max step reached check is
            // processed even after the agent is deactivated by SimulationManager, so we 
            // mark the agent as interrupted only when he is still activated. 
            if (isActivated) interrupted = true;
            OnMaxStepsReached();
        }
    }

    /// <summary>
    /// The method is called if max steps count is reached
    /// </summary>
    public abstract void OnMaxStepsReached();

    /// <summary>
    /// End episode for the agent. Must be called insted of EndEpisode() method
    /// to handle interrupted case!
    /// </summary>
    public virtual void EndAgent()
    {
        if (interrupted)
        {
            interrupted = false;
            EpisodeInterrupted();
        }
        else
        {
            EndEpisode();
        }
    }
}