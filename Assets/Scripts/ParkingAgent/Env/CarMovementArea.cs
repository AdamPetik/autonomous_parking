using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component for managing a moving car (as noise in the learning environment).
/// </summary>
public class CarMovementArea : MonoBehaviour
{
    /// <summary>
    /// Car prefab to be used to spawn a car.
    /// </summary>
    [SerializeField]
    private GameObject carPrefab;
    
    /// <summary>
    /// Car speed.
    /// </summary>    
    [SerializeField]
    private float speed = 10f;
    
    /// <summary>
    /// Moving direciton.
    ///     - 1 forward
    ///     - -1 backward
    /// </summary>
    private int direction = 1;

    /// <summary>
    /// Spawned car game object.
    /// </summary>
    private GameObject car;
    
    /// <summary>
    /// Idicates whether the car is moving or not.
    /// </summary>
    private bool isMoving = false;

    /// <summary>
    /// Max local z coordinate. It is always 5.
    /// </summary>
    private float z = 5f;

    /// <summary>
    /// Initializes the car and positions it inside the plane z coordinate.
    /// <param name="agents"> Game objects of agents. Are used to calculate distances from the car. </param>
    /// <param name="distanceFromAgents"> Minimum required distance between car and agents. </param>
    /// <param name="goForward"> Whether car should go forward at the start. </param>
    /// </summary>
    public void Initialize(List<GameObject> agents, float distanceFromAgents = 10, bool goForward = true)
    {
        StopCar();
        
        direction = goForward ? 1 : -1;

        float minDistance = float.MaxValue;
        Vector3 initPosition;

        // finds random position on z coordinate, which distance from all agents,
        // is larger than distanceFromAgents parameter
        do
        {
            minDistance = float.MaxValue;
            float randomZ = Random.Range(transform.localScale.z*-5, transform.localScale.z*5);
            initPosition = transform.position + transform.forward * randomZ;

            foreach(GameObject agent in agents)
            {
                float tmpDistance = Vector3.Distance(initPosition, agent.transform.position);
                if (tmpDistance < minDistance) minDistance = tmpDistance;
            }
        } while (minDistance <= distanceFromAgents);

        // instatiates the car if it have not been instantiated yet
        if (car == null)
        {
            car = Object.Instantiate(carPrefab, initPosition, transform.rotation);
        }
        else
        {
            car.transform.position = initPosition;
            car.transform.rotation = transform.rotation;
        }
    }

    private void FixedUpdate() {
        if (isMoving) {
            MoveCar();
        }

        if (car != null) 
        {
            // Here it checks whether the car is out of the plane z coodrinate size bound
            // if yes, it inverses its direction
            float carZ = transform.InverseTransformPoint(car.transform.position).z;
            if ( carZ > z && direction == 1)
            {
                direction = -1;
            }
            
            if (carZ < -z && direction == -1)
            {
                direction = 1;
            }
        }
        
    }

    /// <summary>
    /// Starts the car.
    /// </summary> 
    public void StartCar()
    {
        isMoving = true;
    }

    /// <summary>
    /// Calculates the new car position based on the speed, direction and delta time.
    /// </summary> 
    private void MoveCar()
    {
        car.transform.position += car.transform.forward * speed / 3.6f * direction * Time.deltaTime;
    }

    /// <summary>
    /// Stops the car.
    /// </summary> 
    public void StopCar()
    {
        if (isMoving)
        {
            isMoving = false;
        }
    }

    /// <summary>
    /// Removes the car.
    /// </summary> 
    public void RemoveCar()
    {
        StopCar();
        if (car != null)
        {
            Object.Destroy(car);
            car = null;
        }
    }
}
