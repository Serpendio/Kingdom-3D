using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Directions
{
    Up,
    Right,
    Down,
    Left
}

public class Zone
{
    private bool _isSafe = false;

    public bool IsSafe { 
        get { return _isSafe ||
                isCentralZone; } 
        set { _isSafe = value; } 
    }

    // from the perspective of the centre
    public Polar bottomRight, topLeft; 
    public float Width => bottomRight.Theta == topLeft.Theta ? 2 * Mathf.PI : PolarMaths.AngleBetween(bottomRight.Theta, topLeft.Theta);
    public float Height => topLeft.r - bottomRight.r;
    
    // literally just for the safety check & upgrading
    public Gate[] gates;
    public DirtMound[] mounds;
    public List<Zone> neighbouringZones;
    public List<Directions> gateDirections;
    public bool isCentralZone;
    // public bool isBuilt = false; // non-built spaces are automatically not safe edit: this won't work if surrounded but not built
    public bool containsPortal; // portal spaces are automatically not safe
    
    public bool ContainsPoint(Polar coord)
    {
        return  bottomRight.r <= coord.r
                && coord.r <= topLeft.r
            && 
                bottomRight.Theta <= coord.Theta 
                && coord.Theta <= topLeft.Theta;
    }

    public Zone(Polar bottomRight, Polar topLeft)
    {
        this.bottomRight = bottomRight;
        this.topLeft = topLeft;
        neighbouringZones = new();
        gateDirections = new();
    }

    // to be called when zone has walls built, repaired, or destroyed
    // justBuilt is for if repairing/building a zone makes 2 entirely separate zones fixed
    public void CheckSafe(bool justBuilt = false)
    {
        if (LevelController.portals.Count == 0)
        {
            IsSafe = true;
            return;
        }

        // check by flood fill
        HashSet<Zone> set = new() { this };
        bool isSafe = true;

        for (int i = 0; i < neighbouringZones.Count; i++)
        {
            if (gates[i] == null &&
                !set.Contains(neighbouringZones[i])) // O(1) check
            {
                set.Add(neighbouringZones[i]);
                neighbouringZones[i].CheckSafe(ref set, ref isSafe);
                if (isSafe && // don't bother setting isSafe if it's already false
                    (neighbouringZones[i].containsPortal || // containing a portal obviously isn't safe
                    neighbouringZones[i] == LevelController.zones[^1])) // reached outer ring
                {
                    isSafe = false;
                }
            }
        }

        foreach (Zone zone in set)
        {
            zone.IsSafe = isSafe;
        }

        if (justBuilt)
            for (int i = 0; i < neighbouringZones.Count; i++)
            {
                if (!set.Contains(neighbouringZones[i]) 
                    && gateDirections[i] != Directions.Down) // repairing / building doesn't do the lower edge
                {
                    neighbouringZones[i].CheckSafe(); // would need to make sure it then uses the og version,
                                                      // not this new version as that would be infinite loop
                }
            }
    }

    public void CheckSafe(ref HashSet<Zone> set, ref bool isSafe)
    {
        for (int i = 0; i < neighbouringZones.Count; i++)
        {
            if (gates[i] == null &&
                !set.Contains(neighbouringZones[i])) // O(1) check
            {
                set.Add(neighbouringZones[i]);
                neighbouringZones[i].CheckSafe(ref set, ref isSafe);
                if (isSafe && // don't bother setting isSafe if it's already false
                    (neighbouringZones[i].containsPortal || // containing a portal obviously isn't safe
                    neighbouringZones[i] == LevelController.zones[^1])) // reached outer ring
                {
                    isSafe = false;
                }
            }
        }
    }

    public void PlaceMounds()
    {
        for (int i = 0; i < neighbouringZones.Count; i++)
        {
            if (gateDirections[i] == Directions.Up)
            {
                Polar pos = GetGatePos(neighbouringZones[i], Directions.Up);
                var mound = Object.Instantiate(ObjectReferences.Instance.mound, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegreesClockwise, 0), LevelController.Instance.WallsNGates)
                    .GetComponent<DirtMound>();
                mound.linkedZones = new Zone[] { this, neighbouringZones[i] };
            }
        }
    }

    public Polar GetGatePos(Zone otherZone, Directions direction)
    {
        Polar pos = new();

        switch (direction) // refer to PossibleGateConnections.png in the git root
        {
            case Directions.Up:
                pos.r = topLeft.r;

                if (otherZone.topLeft.Theta > topLeft.Theta && topLeft.Theta > otherZone.bottomRight.Theta ||
                    topLeft.Theta > otherZone.bottomRight.Theta && otherZone.bottomRight.Theta > bottomRight.Theta ||
                    otherZone.bottomRight.Theta > bottomRight.Theta && bottomRight.Theta > otherZone.topLeft.Theta) // case 1 (accounting for theta 0 being in middle
                {
                    pos.Theta = PolarMaths.CentreAngle(otherZone.bottomRight.Theta, topLeft.Theta);
                }
                else if (otherZone.topLeft.Theta > bottomRight.Theta && bottomRight.Theta > otherZone.bottomRight.Theta ||
                         bottomRight.Theta > otherZone.bottomRight.Theta && otherZone.bottomRight.Theta > topLeft.Theta ||
                         otherZone.bottomRight.Theta > topLeft.Theta && topLeft.Theta > otherZone.topLeft.Theta) // case 2 (accounting for theta 0 being in middle)
                {
                    pos.Theta = PolarMaths.CentreAngle(bottomRight.Theta, otherZone.topLeft.Theta);
                }
                else // case 3 (we already know they connect, place gate in the middle of the smaller one)
                {
                    pos.Theta = Width > otherZone.Width ?
                                    PolarMaths.CentreAngle(otherZone.bottomRight.Theta, otherZone.topLeft.Theta) :
                                    PolarMaths.CentreAngle(bottomRight.Theta, topLeft.Theta);
                }
                break;
            case Directions.Down:
                pos.r = bottomRight.r;

                if (topLeft.Theta > otherZone.topLeft.Theta && otherZone.topLeft.Theta > bottomRight.Theta ||
                    otherZone.topLeft.Theta > bottomRight.Theta && bottomRight.Theta > otherZone.bottomRight.Theta ||
                    bottomRight.Theta > otherZone.bottomRight.Theta && otherZone.bottomRight.Theta > topLeft.Theta) // case 1 (accounting for theta 0 being in middle
                {
                    pos.Theta = PolarMaths.CentreAngle(bottomRight.Theta, otherZone.topLeft.Theta);
                }
                else if (topLeft.Theta > otherZone.bottomRight.Theta && otherZone.bottomRight.Theta > bottomRight.Theta ||
                         otherZone.bottomRight.Theta > bottomRight.Theta && bottomRight.Theta > otherZone.topLeft.Theta ||
                         bottomRight.Theta > otherZone.topLeft.Theta && otherZone.topLeft.Theta > topLeft.Theta) // case 2 (accounting for theta 0 being in middle)
                {
                    pos.Theta = PolarMaths.CentreAngle(otherZone.bottomRight.Theta, topLeft.Theta);
                }
                else // case 3 (we already know they connect, place gate in the middle of the smaller one)
                {
                    pos.Theta = Width > otherZone.Width ? 
                                    PolarMaths.CentreAngle(otherZone.bottomRight.Theta, otherZone.topLeft.Theta) : 
                                    PolarMaths.CentreAngle(bottomRight.Theta, topLeft.Theta);
                }
                break;
            case Directions.Right: // will need to mimic up if bsp is used
                pos.Theta = bottomRight.Theta;
                pos.r = (topLeft.r + bottomRight.r) / 2;
                break;
            case Directions.Left: // will need to mimic up if bsp is used
                pos.Theta = topLeft.Theta;
                pos.r = (topLeft.r + bottomRight.r) / 2;
                break;
        }

        return pos;
    }

    public void BuildWalls() // had a mental breakdown creating this, excuse the sloppiness
    {
        List<GameObject> allObjects = new();

        #region Up
        // probably not optimised, but it is more readable
        float fullWallLength = PolarMaths.ArcLength(topLeft.r, Width);
        float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
        int numWalls = Mathf.RoundToInt(unroundedNumWalls);
        float widthMultiplier = unroundedNumWalls / numWalls;
        float changePerWall = PolarMaths.SectorAngle(IslandGenerator.wallWidth * widthMultiplier, IslandGenerator.wallWidth); // is in theta
        Polar pos = new(topLeft.r, 0);

        List<Vector2Int> gateIndexes = new(); // x = index in gates, y = index in the full wall
        int gateIndexesIndex = 0; // keeps track of the next index to look at

        for (int i = 0; i < gates.Count(); i++)
        {
            if (gates[i] == null && gateDirections[i] == Directions.Up)
            {
                foreach (DirtMound mound in mounds)
                {
                    if (mound.linkedZones[0] == neighbouringZones[i] || mound.linkedZones[0] == neighbouringZones[i])
                    {
                        int index = Mathf.RoundToInt((PolarMaths.CartesianToPolar(mound.transform.position).Theta - bottomRight.Theta) / changePerWall) - 1; // -1 as indexing starts at 0
                        gateIndexes.Add(new(i, index));
                        break;
                    }
                }
            }
        }
        gateIndexes.OrderBy(i => i[1]);

        List<GameObject> connectedWalls = new();
        for (int i = 0; i < numWalls; i++)
        {
            pos.Theta = bottomRight.Theta + i * changePerWall;
            if (gateIndexesIndex < gateIndexes.Count() && i == gateIndexes[gateIndexesIndex].y)
            {
                gates[gateIndexes[gateIndexesIndex].x] = Object.Instantiate(ObjectReferences.Instance.gate1, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegrees, 0), LevelController.Instance.WallsNGates).GetComponent<Gate>();
                allObjects.Add(gates[gateIndexes[gateIndexesIndex].x].gameObject);

                gateIndexesIndex++;
            }
            else
                connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.wall1, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegrees, 0), LevelController.Instance.WallsNGates));
        }
        foreach (Vector2Int index in gateIndexes)
        {
            gates[index.x].connectedWalls = connectedWalls;
        }
        allObjects.AddRange(connectedWalls);
        #endregion
        #region Sides
        // probably not optimised, but it is more readable
        unroundedNumWalls = Height / IslandGenerator.wallWidth;
        numWalls = Mathf.RoundToInt(unroundedNumWalls);
        widthMultiplier = unroundedNumWalls / numWalls;
        changePerWall = IslandGenerator.wallWidth * widthMultiplier;
        pos = new(0, topLeft.Theta);
        Polar pos2 = new(0, bottomRight.Theta);

        gateIndexes.Clear(); // x = index in gates, y = index in the full wall
        gateIndexes.Add(new(gateDirections.IndexOf(Directions.Left), numWalls / 2));
        gateIndexes.Add(new(gateDirections.IndexOf(Directions.Right), numWalls / 2));

        connectedWalls.Clear();
        List<GameObject> connectedWalls2 = new();

        for (int i = 0; i < numWalls; i++)
        {
            pos.r = pos2.r = bottomRight.r + i * changePerWall;
            if (i == gateIndexes[0].y)
            {
                gates[gateIndexes[0].x] = Object.Instantiate(ObjectReferences.Instance.sideGate1, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegrees + 90, 0), LevelController.Instance.WallsNGates).GetComponent<Gate>();
                gates[gateIndexes[1].x] = Object.Instantiate(ObjectReferences.Instance.sideGate1, PolarMaths.P2V3(pos2),
                    Quaternion.Euler(0, pos2.ThetaDegrees + 90, 0), LevelController.Instance.WallsNGates).GetComponent<Gate>();
                allObjects.Add(gates[gateIndexes[0].x].gameObject);
                allObjects.Add(gates[gateIndexes[1].x].gameObject);
            }
            else
            {
                connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.sideWall1, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegrees + 90, 0), LevelController.Instance.WallsNGates));
                connectedWalls2.Add(Object.Instantiate(ObjectReferences.Instance.sideWall1, PolarMaths.P2V3(pos2),
                    Quaternion.Euler(0, pos2.ThetaDegrees + 90, 0), LevelController.Instance.WallsNGates));
            }
        }
        gates[gateIndexes[0].x].connectedWalls = connectedWalls;
        gates[gateIndexes[1].x].connectedWalls = connectedWalls2;
        allObjects.AddRange(connectedWalls);
        allObjects.AddRange(connectedWalls2);
        #endregion

        allObjects[0] // is guaranteed to be a gate
            .AddComponent<BuildJob>().Setup((int)WallUpgradeHits.level1, 2, allObjects.ToArray(), false);

        // remove the mounds
        foreach (DirtMound mound in mounds)
            Object.Destroy(mound);
        mounds = null;
    }

    public void UpgradeWalls(WallLevels level)
    {

    }

    public void RepairWalls()
    {
        for (int i = 0; i < gateDirections.Count; i++)
        {
            if (gates[i].isDestroyed && gateDirections[i] != Directions.Down)
                gates[i].UpgradeWalls();
        }
    }
}
