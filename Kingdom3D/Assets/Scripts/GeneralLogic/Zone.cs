using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardinalDirections
{
    North,
    East,
    South,
    West
}

public class Zone
{
    public bool isSafe;
    public List<Zone> neighbouringZones;

    // from the perspective of the centre
    public Polar bottomRight, topLeft; 
    
    // literally just for the safety check & upgrading
    public Gate[] gates;
    public CardinalDirections[] gateDirections;

    public bool ContainsPoint(Polar coord)
    {
        return  bottomRight.r <= coord.r
                && coord.r <= topLeft.r
            && 
                bottomRight.theta <= coord.theta 
                && coord.theta <= topLeft.theta;
    }

    public Zone(Polar bottomRight, Polar topLeft)
    {
        this.bottomRight = bottomRight;
        this.topLeft = topLeft;
    }

    bool CheckSafe()
    {
        if (leftSide == null)
        {

        }

        return true;
    }
}
