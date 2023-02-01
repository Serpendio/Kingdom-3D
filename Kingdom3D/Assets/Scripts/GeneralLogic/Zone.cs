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
    public List<DirtMound> mounds;
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
        mounds = new();
        for (int i = 0; i < neighbouringZones.Count; i++)
        {
            if (gateDirections[i] == Directions.Up)
            {
                Polar pos = GetGatePos(this, neighbouringZones[i], Directions.Up);
                var mound = Object.Instantiate(ObjectReferences.Instance.mound, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegreesClockwise, 0), LevelController.Instance.mounds)
                    .GetComponent<DirtMound>();
                mound.linkedZones = new Zone[] { this, neighbouringZones[i] };
                mounds.Add(mound);
            }
        }
    }

    public static Polar GetGatePos(Zone fromZone, Zone toZone, Directions direction)
    {
        Polar pos = new();

        // get the position
        switch (direction) // refer to PossibleGateConnections.png in the git root
        {
            case Directions.Up:
                pos.r = fromZone.topLeft.r;

                if (toZone.topLeft.Theta > fromZone.topLeft.Theta && fromZone.topLeft.Theta > toZone.bottomRight.Theta ||
                    fromZone.topLeft.Theta > toZone.bottomRight.Theta && toZone.bottomRight.Theta > fromZone.bottomRight.Theta ||
                    toZone.bottomRight.Theta > fromZone.bottomRight.Theta && fromZone.bottomRight.Theta > toZone.topLeft.Theta) // case 1 (accounting for theta 0 being in middle
                {
                    pos.Theta = PolarMaths.CentreAngle(toZone.bottomRight.Theta, fromZone.topLeft.Theta);
                }
                else if (toZone.topLeft.Theta > fromZone.bottomRight.Theta && fromZone.bottomRight.Theta > toZone.bottomRight.Theta ||
                         fromZone.bottomRight.Theta > toZone.bottomRight.Theta && toZone.bottomRight.Theta > fromZone.topLeft.Theta ||
                         toZone.bottomRight.Theta > fromZone.topLeft.Theta && fromZone.topLeft.Theta > toZone.topLeft.Theta) // case 2 (accounting for theta 0 being in middle)
                {
                    pos.Theta = PolarMaths.CentreAngle(fromZone.bottomRight.Theta, toZone.topLeft.Theta);
                }
                else // case 3 (we already know they connect, place gate in the middle of the smaller one)
                {
                    pos.Theta = fromZone.Width > toZone.Width ?
                                    PolarMaths.CentreAngle(toZone.bottomRight.Theta, toZone.topLeft.Theta) :
                                    PolarMaths.CentreAngle(fromZone.bottomRight.Theta, fromZone.topLeft.Theta);
                }
                break;
            case Directions.Down:
                pos.r = fromZone.bottomRight.r;

                if (fromZone.topLeft.Theta > toZone.topLeft.Theta && toZone.topLeft.Theta > fromZone.bottomRight.Theta ||
                    toZone.topLeft.Theta > fromZone.bottomRight.Theta && fromZone.bottomRight.Theta > toZone.bottomRight.Theta ||
                    fromZone.bottomRight.Theta > toZone.bottomRight.Theta && toZone.bottomRight.Theta > fromZone.topLeft.Theta) // case 1 (accounting for theta 0 being in middle
                {
                    pos.Theta = PolarMaths.CentreAngle(fromZone.bottomRight.Theta, toZone.topLeft.Theta);
                }
                else if (fromZone.topLeft.Theta > toZone.bottomRight.Theta && toZone.bottomRight.Theta > fromZone.bottomRight.Theta ||
                         toZone.bottomRight.Theta > fromZone.bottomRight.Theta && fromZone.bottomRight.Theta > toZone.topLeft.Theta ||
                         fromZone.bottomRight.Theta > toZone.topLeft.Theta && toZone.topLeft.Theta > fromZone.topLeft.Theta) // case 2 (accounting for theta 0 being in middle)
                {
                    pos.Theta = PolarMaths.CentreAngle(toZone.bottomRight.Theta, fromZone.topLeft.Theta);
                }
                else // case 3 (we already know they connect, place gate in the middle of the smaller one)
                {
                    pos.Theta = fromZone.Width > toZone.Width ? 
                                    PolarMaths.CentreAngle(toZone.bottomRight.Theta, toZone.topLeft.Theta) : 
                                    PolarMaths.CentreAngle(fromZone.bottomRight.Theta, fromZone.topLeft.Theta);
                }
                break;
            case Directions.Right: // will need to mimic up if bsp is used
                pos.Theta = fromZone.bottomRight.Theta;
                pos.r = (fromZone.topLeft.r + fromZone.bottomRight.r) / 2;
                break;
            case Directions.Left: // will need to mimic up if bsp is used
                pos.Theta = fromZone.topLeft.Theta;
                pos.r = (fromZone.topLeft.r + fromZone.bottomRight.r) / 2;
                break;
        }


        // align to walls
        if (direction == Directions.Up || direction == Directions.Down)
        {
            Zone zoneToUse = direction == Directions.Up ? fromZone : toZone;

            float fullWallLength = PolarMaths.ArcLength(zoneToUse.topLeft.r, zoneToUse.Width);
            float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
            int numWalls = Mathf.RoundToInt(unroundedNumWalls);
            float widthMultiplier = unroundedNumWalls / numWalls;
            float changePerWall = PolarMaths.SectorAngle(zoneToUse.topLeft.r, IslandGenerator.wallWidth * widthMultiplier); // is in radians

            pos.Theta = (Mathf.Round((pos.Theta - zoneToUse.bottomRight.Theta) / changePerWall - 0.5f) + 0.5f) * changePerWall + zoneToUse.bottomRight.Theta;
        }
        else
        {

            float fullWallLength = fromZone.Height;
            float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
            int numWalls = Mathf.RoundToInt(unroundedNumWalls);
            float widthMultiplier = unroundedNumWalls / numWalls;
            float changePerWall = IslandGenerator.wallWidth * widthMultiplier;

            pos.r = (Mathf.Round((pos.r - fromZone.bottomRight.r) / changePerWall - 0.5f) + 0.5f) * changePerWall + fromZone.bottomRight.r;
        }


        return pos;
    }

    public static Polar[] GetWallPositions(Zone zone, Directions direction)
    {
        Polar[] allPositions;
        if (direction == Directions.Up || direction == Directions.Down)
        {
            Polar currentPos = direction == Directions.Up ? zone.topLeft : zone.bottomRight;

            float fullWallLength = PolarMaths.ArcLength(currentPos.r, zone.Width);
            float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
            int numWalls = Mathf.RoundToInt(unroundedNumWalls);
            float widthMultiplier = unroundedNumWalls / numWalls;
            float changePerWall = PolarMaths.SectorAngle(currentPos.r, IslandGenerator.wallWidth * widthMultiplier); // is in radians

            allPositions = new Polar[numWalls - 1];
            for (int i = 0; i < numWalls - 1; i++)
            {
                currentPos.Theta = zone.bottomRight.Theta + changePerWall * (i + 0.5f);
                allPositions[i] = currentPos;
            }
        }
        else
        {
            Polar currentPos = direction == Directions.Left ? zone.topLeft : zone.bottomRight;
            
            float fullWallLength = zone.Height;
            float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
            int numWalls = Mathf.RoundToInt(unroundedNumWalls);
            float widthMultiplier = unroundedNumWalls / numWalls;
            float changePerWall = IslandGenerator.wallWidth * widthMultiplier;

            allPositions = new Polar[numWalls];
            for (int i = 0; i < numWalls; i++)
            {
                currentPos.r = zone.bottomRight.r + changePerWall * (i + 0.5f);
                allPositions[i] = currentPos;
            }
        }

        return allPositions;
    }

    public void BuildWalls()
    {
        List<GameObject> allObjects = new();

        #region Up
        int gateIndex = 0;
        Polar[] positions = GetWallPositions(this, Directions.Up);
        List<GameObject> connectedWalls = new();
        int nextMound = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            
            if (nextMound < mounds.Count() && PolarMaths.P2V3(positions[i]).Approximately(mounds[nextMound].transform.position))
            {
                int index = neighbouringZones.IndexOf(mounds[nextMound].linkedZones[1]);
                gates[index] = Object.Instantiate(ObjectReferences.Instance.gate1, PolarMaths.P2V3(positions[i]),
                        Quaternion.Euler(0, positions[i].ThetaDegreesClockwise, 0), LevelController.Instance.wallsNGates).GetComponent<Gate>();
                allObjects.Add(gates[index].gameObject);
                gateIndex = i;
                nextMound++;
            }
            else 
                connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.wall1, PolarMaths.P2V3(positions[i]),
                    Quaternion.Euler(0, positions[i].ThetaDegreesClockwise, 0), LevelController.Instance.wallsNGates));
        }
        for (int i = 0; i < gates.Length; i++)
            if (gateDirections[i] == Directions.Up)
                gates[i].connectedWalls = connectedWalls;
        allObjects.AddRange(connectedWalls);
        #endregion

        #region Sides
        if (!isCentralZone)
        {
            //TODO: remove the + 90f once side walls have been properly created

            int index = gateDirections.IndexOf(Directions.Left);
            if (gates[index] == null)
            {
                positions = GetWallPositions(this, Directions.Left);
                connectedWalls.Clear();
                for (int i = 0; i < positions.Length; i++)
                {
                    if (i == positions.Length / 2)
                    {
                        gates[index] = Object.Instantiate(ObjectReferences.Instance.sideGate1, PolarMaths.P2V3(positions[i]),
                                Quaternion.Euler(0, positions[i].ThetaDegreesClockwise, 0), LevelController.Instance.wallsNGates).GetComponent<Gate>();
                        allObjects.Add(gates[index].gameObject);
                    }
                    else
                        connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.wall1, PolarMaths.P2V3(positions[i]),
                            Quaternion.Euler(0, positions[i].ThetaDegreesClockwise + 90f, 0), LevelController.Instance.wallsNGates));
                }
                gates[index].connectedWalls = connectedWalls;
            }
            allObjects.AddRange(connectedWalls);

            index = gateDirections.IndexOf(Directions.Right);
            if (gates[index] == null)
            {
                positions = GetWallPositions(this, Directions.Left);
                connectedWalls.Clear();
                for (int i = 0; i < positions.Length; i++)
                {
                    if (i == positions.Length / 2)
                    {
                        gates[index] = Object.Instantiate(ObjectReferences.Instance.sideGate1, PolarMaths.P2V3(positions[i]),
                                Quaternion.Euler(0, positions[i].ThetaDegreesClockwise, 0), LevelController.Instance.wallsNGates).GetComponent<Gate>();
                        allObjects.Add(gates[index].gameObject);
                    }
                    else
                        connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.wall1, PolarMaths.P2V3(positions[i]),
                            Quaternion.Euler(0, positions[i].ThetaDegreesClockwise + 90f, 0), LevelController.Instance.wallsNGates));
                }
                gates[index].connectedWalls = connectedWalls;
            }
            allObjects.AddRange(connectedWalls);
        }
        #endregion

        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null && gates[i].connectedZones == null)
            {
                gates[i].connectedZones = new Zone[] { this, neighbouringZones[i] };
                neighbouringZones[i].gates[neighbouringZones[i].neighbouringZones.IndexOf(this)] = gates[i];
            }
        }

        allObjects[gateIndex].AddComponent<BuildJob>().Setup((int)WallUpgradeHits.level1, 2, allObjects.ToArray(), false);

        // remove the mounds
        foreach (DirtMound mound in mounds)
            Object.Destroy(mound.gameObject);
        mounds = null;
    }

    public void BuildWallsOld()
    {
        List<GameObject> allObjects = new();

        #region Up
        // probably not optimised, but it is more readable
        float fullWallLength = PolarMaths.ArcLength(topLeft.r, Width);
        float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
        int numWalls = Mathf.RoundToInt(unroundedNumWalls);
        float widthMultiplier = unroundedNumWalls / numWalls;
        float changePerWall = PolarMaths.SectorAngle(IslandGenerator.wallWidth * widthMultiplier, IslandGenerator.wallWidth); // is in radians
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
                    Quaternion.Euler(0, pos.ThetaDegrees, 0), LevelController.Instance.wallsNGates).GetComponent<Gate>();
                allObjects.Add(gates[gateIndexes[gateIndexesIndex].x].gameObject);

                gateIndexesIndex++;
            }
            else
                connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.wall1, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegrees, 0), LevelController.Instance.wallsNGates));
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
                    Quaternion.Euler(0, pos.ThetaDegrees + 90, 0), LevelController.Instance.wallsNGates).GetComponent<Gate>();
                gates[gateIndexes[1].x] = Object.Instantiate(ObjectReferences.Instance.sideGate1, PolarMaths.P2V3(pos2),
                    Quaternion.Euler(0, pos2.ThetaDegrees + 90, 0), LevelController.Instance.wallsNGates).GetComponent<Gate>();
                allObjects.Add(gates[gateIndexes[0].x].gameObject);
                allObjects.Add(gates[gateIndexes[1].x].gameObject);
            }
            else
            {
                connectedWalls.Add(Object.Instantiate(ObjectReferences.Instance.sideWall1, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegrees + 90, 0), LevelController.Instance.wallsNGates));
                connectedWalls2.Add(Object.Instantiate(ObjectReferences.Instance.sideWall1, PolarMaths.P2V3(pos2),
                    Quaternion.Euler(0, pos2.ThetaDegrees + 90, 0), LevelController.Instance.wallsNGates));
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
