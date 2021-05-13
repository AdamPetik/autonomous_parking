using System.IO;
using UnityEngine;

/// <summary>
/// Component which evaluates parking accuracy (rotation deviation and
/// x coordinate devation) of agents.
/// </summary>
public class ParkingAccuracyStats : ParkingAgentObserver
{
    
    /// <summary>
    /// Mean of rotation deviation.
    /// </summary>
    private double rotationDeviationMean = 0;
    
    /// <summary>
    /// Mean of X coordinate distance deviation (from center of a parking spot).
    /// </summary>
    private double xDeviationMean = 0;

    /// <summary>
    /// Represents how many times the agent parked successfuly.
    /// </summary>
    private int parked = 0;

    /// <summary>
    /// Represents how many successful parking times are needed to generate the stats.
    /// </summary>
    [SerializeField]
    private int maxParked = 1000;
    
    /// <summary>
    /// Stat id which is used for name of generated file.
    /// </summary>
    [SerializeField]
    private string statId = "";

    public override void OnCollision(Collider collider, ParkingAgent agent) {}

    public override void OnMaxStepsReached(ParkingAgent agent) {}

    public override void OnParked(Collider collider, ParkingAgent agent)
    {
        parked++;
        // calculates the rotation deviation
        double rotationDeviation = Utils.YRotationDiff(agent.transform, collider.transform);
        rotationDeviationMean = CalcNewMean(rotationDeviationMean, rotationDeviation);
        // Debug.Log($"rotationDiffMean: {rotationDiffMean}");

        // calculates the x coordinate deviation
        double xDeviation = Utils.XPositionDiff(agent.transform, collider.transform);
        // Debug.Log($"xDiff: {xDiff}");
        xDeviationMean = CalcNewMean(xDeviationMean, xDeviation);
        // Debug.Log($"xDiffMean: {xDiffMean}");
        HandlePrintScore();
    }

    /// <summary>
    /// Calculates the new mean by parked count.
    /// </summary>
    private double CalcNewMean(double oldMean, double newValue)
    {
        if (parked == 0) return 0;
        return oldMean + (1.0 / parked) * (newValue - oldMean);
    }

    /// <summary>
    /// Calls the method PrintScore() if desired number of successfull parking times is reached.
    /// </summary>
    private void HandlePrintScore()
    {
        if (parked == maxParked) PrintScore();
    }

    /// <summary>
    /// Prints the stats to the log console and creates a stats file.
    /// </summary>
    public void PrintScore()
    {
        // prints the stats to the log
        string text = $"Parking Accuracy Score:\n\tParked: {parked.ToString()}\n\tRotation mean: {rotationDeviationMean.ToString("F2")}\n\tX diff mean: {xDeviationMean.ToString("F2")}";
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

        string filePath = $"{directory}/parkingAccuracyStats_{statId}.txt";
        // creates stats file in Assets/StatResults folder.
        StreamWriter writer = new StreamWriter(filePath, false);
        writer.Write(text);
        writer.Close();  
    }
}