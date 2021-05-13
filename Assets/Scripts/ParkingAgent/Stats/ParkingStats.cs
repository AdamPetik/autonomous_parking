using System.IO;
using UnityEngine;

/// <summary>
/// Component which evaluates parking stats (number of successful parking,
/// number of collisions, number of collision to the other agents or moving cars,
/// number of interruptions) of agents.
/// </summary>
public class ParkingStats : ParkingAgentObserver
{   
    /// <summary>
    /// Number of interruption.
    /// </summary>
    private int interrupted = 0;
    
    /// <summary>
    /// Represents how many times the agent parked successfuly.
    /// </summary>
    private int parked = 0;

    /// <summary>
    /// Number of collisions.
    /// </summary>
    private int collided = 0;
    
    /// <summary>
    /// Number of collisions to the other agents or moving cars.
    /// </summary>
    private int collidedToMovingCar = 0;

    /// <summary>
    /// Total number of recorded episodes.
    /// </summary>
    public int EpisodesCount { get { return interrupted + parked + collided; }}
    
    /// <summary>
    /// Number of collisions to the other agents or moving cars.
    /// </summary>
    [SerializeField]
    private int maxEpisodes = 1000;
    
    /// <summary>
    /// Stat id which is used for name of generated file.
    /// </summary>
    [SerializeField]
    private string statId = "";

    public override void OnCollision(Collider collider, ParkingAgent agent)
    {
        collided++;
        if (collider.tag == "MovingCar" || collider.tag == "Agent") collidedToMovingCar++;
        HandlePrintScore();
    }

    public override void OnMaxStepsReached(ParkingAgent agent)
    {
        interrupted++;
        HandlePrintScore();
    }

    public override void OnParked(Collider collider, ParkingAgent agent)
    {
        parked++;
        HandlePrintScore();
    }

    /// <summary>
    /// Calls the method PrintScore() if desired number of successfull parking times is reached.
    /// </summary>
    private void HandlePrintScore()
    {
        if(EpisodesCount == maxEpisodes) PrintScore();
    }

    /// <summary>
    /// Prints the stats to the log console and creates a stats file.
    /// </summary>
    public void PrintScore()
    {
        
        float scoreWithInterrupted = (float) parked / EpisodesCount;
        float scoreWithoutInterrupted = (float) parked / (EpisodesCount - interrupted);

        string text = $"Parking Score:\n\tParked: {parked.ToString()}\n\tCollided: {collided.ToString()} (MovingCar collisions: {collidedToMovingCar.ToString()})\n\tInterrupted: {interrupted.ToString()}\n\n\tScore with interrupted: {scoreWithInterrupted.ToString("F2")}\n\tScore without interrupted: {scoreWithoutInterrupted.ToString("F2")}";
        
        // prints the stats to the log
        Debug.Log(text); 
        
        string directory = "Assets/StatResults";
        // creates directory if it is not exists
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

        }
        catch (IOException ex)
        {
            Debug.Log(ex.Message);
        }

        // creates stats file in Assets/StatResults folder.
        string filePath = $"{directory}/parkingStats_{statId}.txt";
        StreamWriter writer = new StreamWriter(filePath, false);
        writer.Write(text);
        writer.Close();
    }
}