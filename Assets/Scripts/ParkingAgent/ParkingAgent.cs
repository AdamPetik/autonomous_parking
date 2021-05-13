using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Component representing parking agent.  
/// </summary>
public class ParkingAgent : BaseAgent
{
    private CarController carController;
    
    [SerializeField]
    private SimulationManager simManager;
    public SimulationManager SimManager{get {return simManager;} }

    public float maxMotorTorque = 1000;
    public float maxSteerAngle = 30;

    protected float actionReward = -0.001f;
    protected float collisionReward = -0.5f;
    protected float goalReward = 1;

    protected float distantAccuracyMaxReward = 0.2f;
    protected float rotationAccuracyMaxReward = 0.3f;
    protected float parkedSpeedReward = 0.3f;

    private DistanceReward distanceReward;

    protected float reverseFactor = 0f;

    private List<ParkingAgentObserver> observers = new List<ParkingAgentObserver>();

    private void Awake()
    {
        carController = GetComponent<CarController>();
        distanceReward = new DistanceReward(this);
        
        if (simManager == null)
        {
            Debug.Log("Simulation manager was not assigned - finding component in parents objects");
            simManager = gameObject.GetComponentInParent<SimulationManager>();
            if (simManager == null)
            {
                throw new System.Exception("Simulation manager was not found in parents objects.");
            }
        }
    }

    protected override void PreStep(int academyStepCount)
    {
        base.PreStep(academyStepCount);

        // action and distance rewards are added here, becasue this method will
        // be called after the selected action was applied. 
        if (IsActivated)
        {
            AddReward(GetFactorByReverse() * actionReward);
            distanceReward.AddDistanceReward();
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        carController.StopCar();
        
        simManager.Initialize(this, environmentParameters);
        
        SetupRewards();
    }

    public override void OnActionReceived(float[] act)
    {
        // actions are clamped and applied
        float clampedMotorAction = Mathf.Clamp(act[0], -1f, 1f);
        float clampedSteerAction = Mathf.Clamp(act[1], -1f, 1f);
        
        carController.motorTorque = maxMotorTorque * clampedMotorAction;
        carController.steerAngle = maxSteerAngle * clampedSteerAction;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carController.motorTorque);
        sensor.AddObservation(carController.steerAngle);
        sensor.AddObservation(carController.GetSpeed());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
    }

    protected virtual void OnCollisionEnter(Collision other) {
        // checks for collisions only if the agent is activated
        if (IsActivated)
        {
            string tag = other.collider.tag;
            if (tag == "Obstacle" || tag == "Car" || tag == "MovingCar" || tag == "Ground" || tag == "Agent" )
            {
                // if collided, collision reward is added and observers are notified.
                AddReward(collisionReward);
                observers.ForEach((ParkingAgentObserver o) => o.OnCollision(other.collider, this));
                
                // notifies the simulation manager that the agent want to end the episode
                NotifyEnd();
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other) {
        if (IsActivated)
        {
            if(other.tag == "Goal")
            {
                Debug.Log("Parked");
                // adds rewards and notifies the observers
                AddReward(GetFactorByParkedReversed(other.gameObject.transform) * goalReward);
                AddParkingAccuracyReward(other.transform);
                AddParkedSpeedReward();
                observers.ForEach((ParkingAgentObserver o) => o.OnParked(other, this));
                
                // notifies the simulation manager that the agent want to end the episode
                NotifyEnd();
            }
        }
    }
    private void AddParkedSpeedReward()
    {
        float speed = Mathf.Abs(carController.GetSpeed());
        if (parkedSpeedReward > 0)
        {
            AddReward(parkedSpeedReward - 0.03f * speed);
        }
    }

    private void AddParkingAccuracyReward(Transform triggerTransform)
    {
        Transform nearestSpot = triggerTransform;

        float rotation = Utils.YRotationDiff(transform, nearestSpot);
        float rotationReward = rotationAccuracyMaxReward * 1f / Mathf.Clamp(rotation, 1f, 90f);

        float distance = Utils.XPositionDiff(transform, nearestSpot);
        float distanceReward = distantAccuracyMaxReward * 0.1f / Mathf.Clamp(distance, 0.1f, 1f);

        float accuracyReward = distanceReward + rotationReward;
        AddReward(accuracyReward);
    }

    /// <summary>
    /// Loads the rewards from the config file.  
    /// </summary>
    protected virtual void SetupRewards()
    {
        goalReward = environmentParameters.GetWithDefault("goal_reward", goalReward);
        collisionReward = environmentParameters.GetWithDefault("collision_reward", collisionReward);
        actionReward = environmentParameters.GetWithDefault("action_reward", actionReward);
        distantAccuracyMaxReward = environmentParameters.GetWithDefault("distant_accuracy_max_reward", distantAccuracyMaxReward);
        rotationAccuracyMaxReward = environmentParameters.GetWithDefault("rotation_accuracy_max_reward", rotationAccuracyMaxReward);
        parkedSpeedReward = environmentParameters.GetWithDefault("parked_speed_reward", parkedSpeedReward);
        reverseFactor = environmentParameters.GetWithDefault("reverse_factor", reverseFactor);
        distanceReward.Initialize(environmentParameters, actionReward);
    }

    public override void Activate()
    {
        base.Activate();
        // after activation it is good to update last saved distance from
        // nearest parking spot by calling UpdateCachedDistance() method
        distanceReward.UpdateCachedDistance();
    }

    /// <summary>
    /// Stops the car and notifies the simulation manager that agent want to end the episode.  
    /// </summary>
    protected void NotifyEnd()
    {
        carController.StopCar();
        simManager.AgentEnds(this);
    }

    public override void OnMaxStepsReached()
    {
        observers.ForEach((ParkingAgentObserver o) => o.OnMaxStepsReached(this));
        // on max steps reached, environment will be reseted by calling simManager.Reset().
        simManager.Reset();
    }
    
    
    public void RegisterObserver(ParkingAgentObserver observer)
    {
        observers.Add(observer);
    }

    public void UnregisterObserver(ParkingAgentObserver observer)
    {
        observers.Remove(observer);
    }

    /// <summary>
    /// Gets the factor by car moving direction.  
    /// </summary>
    private float GetFactorByReverse(float defaultFactor = 1)
    {
        float factor = defaultFactor;
        if(carController.Reverse)
        {
            factor += reverseFactor;
        }
        return factor;
    }

    /// <summary>
    /// Gets the factor by parked car orientation.  
    /// </summary>
    private float GetFactorByParkedReversed(Transform parkingSpot, float defaultFactor = 1)
    {
        float factor = defaultFactor;
        if (ParkedReversed(parkingSpot))
        {
            factor += reverseFactor;
        }
        return factor;
    }

    /// <summary>
    /// Checks whether car was parked by reversing.  
    /// </summary>
    private bool ParkedReversed(Transform parkingSpot)
    {
        return Vector3.Dot(parkingSpot.forward, transform.forward) > 0 ? true : false;
    }
}
