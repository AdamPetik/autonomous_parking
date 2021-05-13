using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// Component responsible for initialization of environment at the begining of every episode.
/// Can be used alone or with conjunction with the SimulationManager.
/// </summary>
public class EnvManager : MonoBehaviour
{
    /// <summary>
    /// Contains prefabs to be instantiated in parking spots.
    /// </summary>
    [SerializeField]
    private List<GameObject> staticCarPrefabs = new List<GameObject>();

    /// <summary>
    /// Default number of free parking spots in one parking section.
    /// </summary>
    [SerializeField]
    private int defaultParkingSectionFreeSpaces = 2;

    /// <summary>
    /// Actual number of free parking spots in one parking section.
    /// </summary>
    private int parkingSectionFreeSpaces;

    /// <summary>
    /// Default probability representig the chance of spawning the agent
    /// in front of a free parking spot.
    /// </summary>
    [SerializeField]
    private float defaultSpawnInFrontOfSpot = 1;

    /// <summary>
    /// Default flag representing whether to spawn moving cars or not.
    ///     1 - will spawn
    ///     0 - will not spawn
    /// </summary>
    [SerializeField]
    private int defaultSpawnStaticMovementCars = 0;

    /// <summary>
    /// Default max agent car rotation used when spawned in front of free parking spot.
    /// </summary> 
    [SerializeField]
    private float defaultMaxRotation = 90;

    /// <summary>
    /// Default rotation offset of agent car rotation when spawned in front of free spot.
    /// (minimal rotation)
    /// </summary>
    [SerializeField]
    private float defaultRotationOffset = 0;
    
    /// <summary>
    /// Contains game objects of cars occupying the parking spot.
    /// </summary>
    protected List<GameObject> parkedStaticCars;
    
    private List<ParkingSpotsSection> spotSections;

    private List<AreaPositioner> areaPositioners;

    private List<CarMovementArea> carMovementAreas;

    /// <summary>
    /// Parking spots that are not occupied by static car.
    /// </summary>
    private List<ParkingSpot> freeSpots;
    
    /// <summary>
    /// List buffer to store already relocated agents.
    /// </summary>
    private List<GameObject> relocatedAgents;

    private void Awake()
    {
        parkedStaticCars = new List<GameObject>();
        // please note ParkingSpotsSection-s, AreaPositioner-s and CarMovementArea-s are collected
        // from children game objects
        spotSections = new List<ParkingSpotsSection>(gameObject.GetComponentsInChildren<ParkingSpotsSection>());
        areaPositioners = new List<AreaPositioner>(gameObject.GetComponentsInChildren<AreaPositioner>());
        carMovementAreas = new List<CarMovementArea>(gameObject.GetComponentsInChildren<CarMovementArea>());

        freeSpots = new List<ParkingSpot>();
        relocatedAgents = new List<GameObject>();
    }

    /// <summary>
    /// Initializes agents in the environment.
    /// </summary>
    /// <param name="agents"> List of agents to be initialized. </param> 
    /// <param name="envParams"> Environment parameters from config file. </param> 
    public void InitializeAgents(List<ParkingAgent> agents, EnvironmentParameters envParams)
    {
        parkingSectionFreeSpaces = (int) envParams.GetWithDefault("parking_section_free_spaces", (float) defaultParkingSectionFreeSpaces);
        DeleteCars();
        InitializeCars();
        
        // relocates all agents
        agents.ForEach(delegate (ParkingAgent agent)
        {
            RelocateAgent(agent.gameObject, envParams);
            relocatedAgents.Add(agent.gameObject);
        });
        relocatedAgents.Clear();
        
        // removes all moving cars
        carMovementAreas.ForEach(carMovementArea => carMovementArea.RemoveCar());

        
        // if static movement cars are required, initializes them
        if (1 == envParams.GetWithDefault("spawn_static_movement_cars", (float) defaultSpawnStaticMovementCars))
        {
            // selects random areas and initializes them
            List<CarMovementArea> selectedCarMovementAreas = 
                    carMovementAreas.SelectRandomItemsWithoutReplacement(Random.Range(0, carMovementAreas.Count + 1));

            selectedCarMovementAreas.ForEach(delegate (CarMovementArea carMovementArea)
            {
                List<GameObject> agentGOs = agents.Map<ParkingAgent, GameObject>(agent => agent.gameObject);

                carMovementArea.Initialize(agentGOs);
                carMovementArea.StartCar();
            });
        }   
    }

    /// <summary>
    /// Relocates agent based on positions of already relocated agents.
    /// </summary>
    /// <param name="agent"> Agent to be relocated. </param> 
    /// <param name="envParams"> Environment parameters from config file. </param> 
    public void RelocateAgent(GameObject agent, EnvironmentParameters envParams)
    {
        
        float spawnInFrontOfSpotRatio = envParams.GetWithDefault("spawn_in_front_of_spot", defaultSpawnInFrontOfSpot);
        bool doSpawnInFrontOfSpot = Random.value > spawnInFrontOfSpotRatio ? false : true;

        // In this case, only one agent from one learning environment can be placed in front of parking spot,
        // because we do not check whether some other agent have not been already relocated in front of selected
        // free parking spot. It is restrictive but this functionality was not needed.
        if (doSpawnInFrontOfSpot && relocatedAgents.Count < 1)
        {
            float maxRotation = envParams.GetWithDefault("max_rotation", defaultMaxRotation);
            float rotationOffset = envParams.GetWithDefault("rotation_offset", defaultRotationOffset);

            // selects random flag for forward or backward orientation
            int forward = Random.Range(0, 2);

            // selects random free parking spot
            Transform parkingSpot = freeSpots.SelectRandom().transform;

            // calculates y rotation
            float toRotate = Utils.RandomSign() * (rotationOffset + Random.Range(0f, maxRotation - rotationOffset));
            float yRotation = parkingSpot.rotation.eulerAngles.y + 180*forward + toRotate;

            // updates agent position and rotation with new values
            Vector3 newPosition = parkingSpot.forward * 7 + parkingSpot.position;
            Quaternion newRotation = Quaternion.Euler(0, yRotation, 0);
            agent.transform.position = newPosition;
            agent.transform.rotation = newRotation;
        }
        else
        {            
            // WARNING - we can not use large amout of agents in one learning environment so that it does not happen,
            // that on the same area more agents would like to be placed and if there is not enought space for all
            // agents, it will create infinity loop.
            var area = areaPositioners.SelectRandom();
            area.Reposition(agent.gameObject, relocatedAgents, 5, Random.value > 0.5 ? 0 : 180);
        }
    }

    /// <summary>
    /// Initializes static cars in parking spots.
    /// </summary>
    protected void InitializeCars()
    {
        freeSpots.Clear();
        spotSections.ForEach(InitializeCarsOnParkingSection);
        
        // it is needed to enable triggers because they are disabled after agent parked
        freeSpots.ForEach(freeSpot => freeSpot.EnableTrigger());
    }
    
    /// <summary>
    /// Initializes static cars in one parking spot and selects free spaces and insersts
    /// them into freeSpaces member variable.
    /// </summary>
    /// <param name="parkingSection"> Section where to initialize static cars </param>
    private void InitializeCarsOnParkingSection(ParkingSpotsSection parkingSection)
    {
        // gets list of "parkable spots" - in those spots can agent parks
        List<ParkingSpot> parkingSpots = parkingSection.ParkableSpots;

        // selects random free spots from parking spots and inserts them to the freeSpots member variable
        // note that selected free spots are removed from parkingSpots list
        SelectRandomFreeSpots(ref parkingSpots);
        
        GameObject prefab = staticCarPrefabs.SelectRandom();

        foreach (ParkingSpot parkingSpot in parkingSpots)
        {
            // in parkingSpots remains spots ready for ocupation by static cars
            parkedStaticCars.Add(Object.Instantiate(prefab, parkingSpot.gameObject.transform.position, parkingSpot.gameObject.transform.rotation));
            // also, triggers are disabled in those spots
            parkingSpot.DisableTrigger();
        }

    }

    /// <summary>
    /// Selects random free spots from parking spots inserts them to the freeSpots member variable.
    /// </summary>
    /// <param name="items"> List of parking spots. Selected free spots are removed from this parameter. </param>
    private void SelectRandomFreeSpots(ref List<ParkingSpot> items)
    {
        for (int i = 0; i < parkingSectionFreeSpaces; i++)
        {
            int removeAt = Random.Range(0, items.Count);
            // adds selected free parking spot to the freeSpots and remove it from items parameter
            freeSpots.Add(items[removeAt].gameObject.GetComponent<ParkingSpot>());
            items.RemoveAt(removeAt);
        }
    }

    /// <summary>
    /// Should be called when all agents want to end episode.
    /// </summary>
    /// <param name="agents"> Ended agents. </param>
    public void AgentsEnd(List<ParkingAgent> agents)
    {
        // moving cars are stopped
        carMovementAreas.ForEach(carMovementArea => carMovementArea.StopCar());
    }

    /// <summary>
    /// Finds nearest free parking spot. Default distance metric is Euclidean.
    /// </summary>
    /// <param name="carAgentPosition"> Position from which to get nearest spot. </param>
    /// <param name="useManhattanDistance"> Whether to use Manhattan distance instead of Euclidean.</param>
    /// <returns> Nearest free parking spot. </returns>
    public ParkingSpot GetNearestSpot(Vector3 carAgentPosition, bool useManhattanDistance = false)
    {
        ParkingSpot nearestSpot = null;

        float lowestDistance = -1;
        float distance;

        foreach(ParkingSpot parkingSpot in freeSpots)
        {
            if (!parkingSpot.Enabled) continue; // there will never be a lot of disabled free parking spots, so it is okey to filter it like that
            
            if (useManhattanDistance)
            {
                distance = Utils.ManhattanDistanceFromVectors(carAgentPosition, parkingSpot.transform.position);
            }
            else
            {
                distance = Vector3.Distance(parkingSpot.transform.position, carAgentPosition);
            }

            if (lowestDistance == -1 || distance <= lowestDistance)
            {
                lowestDistance = distance;
                nearestSpot = parkingSpot;
            }
        }

        // if there is no free parking spot available, error is thrown 
        if (nearestSpot == null) throw new System.Exception("Not enough free parking spots");

        return nearestSpot;
    }

    /// <summary>
    /// calculates Manhattan distance from nearest parking spot. 
    /// </summary>
    /// <param name="agentTransform"> Transform from which to calculate distance. </param>
    /// <returns> Manhattan distance </returns>
    public float NearestPSpotManhattanDistance(Transform agentTransform) {
        Transform spotTransform = GetNearestSpot(agentTransform.position, useManhattanDistance: true).transform;
        float distance = Utils.ManhattanDistance(transform, spotTransform, agentTransform);
        return distance;
    }

    /// <summary>
    /// calculates Manhattan distance from nearest parking spot trigger. 
    /// </summary>
    /// <param name="agentTransform"> Transform from which to calculate distance. </param>
    /// <returns> Manhattan distance </returns>
    public float NearestPSpotTriggerManhattanDistance(Transform agentTransform) {
        Transform triggerTransform = GetNearestSpot(agentTransform.position, useManhattanDistance: true).GetTriggerTransform();
        float distance = Utils.ManhattanDistance(transform, triggerTransform, agentTransform);
        return distance;
    }

    /// <summary>
    /// Destroys static cars game objects. 
    /// </summary>
    protected void DeleteCars()
    {
        parkedStaticCars.ForEach(staticCar => Object.Destroy(staticCar));
    }
}
