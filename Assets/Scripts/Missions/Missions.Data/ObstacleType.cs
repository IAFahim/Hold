using System;

namespace Missions.Missions.Data
{
    [Serializable]
    public enum ObstacleType
    {
        Static,           // Static obstacles (debris, crates)
        Jump,            // Low barriers/ramps requiring jumps
        Slide,           // Low-hanging pipes/vents requiring slides
        MovingDrones,    // Slow-moving drones that move predictably
        IncomingTrain,   // Train approaching on player's track
        DepartingTrain,  // Departing train (temporary lane block)
        AdjacentTrain,   // Train on adjacent track
        LaserGrid,       // Cycling/moving laser beams
        CashCollectibles // Dense lines/waves of bonus cash
    }
}