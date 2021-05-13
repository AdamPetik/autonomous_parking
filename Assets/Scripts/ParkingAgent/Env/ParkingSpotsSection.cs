using System.Collections.Generic;

public class ParkingSpotsSection : Collector<ParkingSpot>
{   
    private List<ParkingSpot> parkableSpots;
    
    /// <summary>
    /// Gets only "parkable" spots from items (all parking spots in this section).
    /// </summary>
    public List<ParkingSpot> ParkableSpots
    {
        get
        {
            if (parkableSpots == null)
            {
                parkableSpots = Items.FindAll(item => item.IsParkable);
            }
            return new List<ParkingSpot>(parkableSpots);
        }
    }
}
