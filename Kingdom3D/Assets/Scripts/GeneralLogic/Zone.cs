using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Gate;
using UnityEngine.InputSystem;

public enum Directions
{
    Up,
    Right,
    Down,
    Left
}

public class Zone
{
    public class NeighbourInfo
    {
        public Gate gate;
        public Directions direction;
        public Zone neighbour;

        public NeighbourInfo(Gate gate, Directions direction, Zone neighbour)
        {
            this.gate = gate;
            this.direction = direction;
            this.neighbour = neighbour;
        }
    }


    private bool _isSafe = false;
    private readonly int zoneIndex;

    public bool IsSafe { 
        get { return _isSafe ||
                isCentralZone; } 
        set { _isSafe = value; Map.Instance.UpdateSafety(zoneIndex); } 
    }

    // from the perspective of the centre
    public Polar bottomRight, topLeft; 
    public float Width => bottomRight.Theta == topLeft.Theta ? 2 * Mathf.PI : PolarMaths.AngleBetween(bottomRight.Theta, topLeft.Theta);
    public float Height => topLeft.r - bottomRight.r;

    public List<DirtMound> mounds;
    public List<NeighbourInfo> neighbourInfos;
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

    public Zone(Polar bottomRight, Polar topLeft, int zoneIndex)
    {
        this.bottomRight = bottomRight;
        this.topLeft = topLeft;
        this.zoneIndex = zoneIndex;
        neighbourInfos = new List<NeighbourInfo>();
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

        foreach (NeighbourInfo info in neighbourInfos)
        {
            if (info.gate == null &&
                !set.Contains(info.neighbour)) // O(1) check
            {
                set.Add(info.neighbour);
                info.neighbour.CheckSafe(ref set, ref isSafe);
                if (isSafe && // don't bother setting isSafe if it's already false
                    (info.neighbour.containsPortal || // containing a portal obviously isn't safe
                    info.neighbour == LevelController.zones[^1])) // reached outer ring
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
            foreach (NeighbourInfo info in neighbourInfos)
            {
                if (!set.Contains(info.neighbour)
                    && info.direction != Directions.Down) // repairing / building doesn't do the lower edge
                {
                    info.neighbour.CheckSafe(); // would need to make sure it then uses the og version (false),
                                                      // not this new version as that would be infinite loop
                }
            }
    }

    public void CheckSafe(ref HashSet<Zone> set, ref bool isSafe)
    {
        foreach (NeighbourInfo info in neighbourInfos)
        {
            if (info.gate == null &&
                !set.Contains(info.neighbour)) // O(1) check
            {
                set.Add(info.neighbour);
                info.neighbour.CheckSafe(ref set, ref isSafe);
                if (isSafe && // don't bother setting isSafe if it's already false
                    (info.neighbour.containsPortal || // containing a portal obviously isn't safe
                    info.neighbour == LevelController.zones[^1])) // reached outer ring
                {
                    isSafe = false;
                }
            }
        }
    }

    public void PlaceMounds()
    {
        mounds = new();
        for (int i = 0; i < neighbourInfos.Count; i++)
        {
            if (neighbourInfos[i].direction == Directions.Up)
            {
                Polar pos = GetMoundPos(neighbourInfos[i]);
                var mound = Object.Instantiate(ObjectReferences.Instance.mound, PolarMaths.P2V3(pos),
                    Quaternion.Euler(0, pos.ThetaDegreesClockwise, 0), LevelController.Instance.mounds)
                    .GetComponent<DirtMound>();
                mound.connectionInfo = new(this, i, neighbourInfos[i].neighbour, -1);
                mounds.Add(mound);
            }
        }
    }

    public Polar GetMoundPos(NeighbourInfo neighbour)
    {
        float fullAngle = PolarMaths.AngleBetween(bottomRight.Theta, neighbour.neighbour.bottomRight.Theta) + PolarMaths.AngleBetween(neighbour.neighbour.bottomRight.Theta, topLeft.Theta);
        bool rightInside = Mathf.Approximately(fullAngle, Width) && fullAngle <= 2 * Mathf.PI;
        fullAngle = PolarMaths.AngleBetween(bottomRight.Theta, neighbour.neighbour.topLeft.Theta) + PolarMaths.AngleBetween(neighbour.neighbour.topLeft.Theta, topLeft.Theta);
        bool leftInside = Mathf.Approximately(fullAngle, Width) && fullAngle <= 2 * Mathf.PI;
        float rightTheta = rightInside ? neighbour.neighbour.bottomRight.Theta : bottomRight.Theta;
        float leftTheta = leftInside ? neighbour.neighbour.topLeft.Theta : topLeft.Theta;
        Polar startPos = new(
        neighbour.direction == Directions.Up ? topLeft.r : bottomRight.r,
        rightTheta
        );

        float fullWallLength = PolarMaths.ArcLength(startPos.r, PolarMaths.AngleBetween(rightTheta, leftTheta));
        float unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
        int numWalls = Mathf.RoundToInt(unroundedNumWalls);
        float widthMultiplier = unroundedNumWalls / numWalls;
        Polar changePerWall = new (0, PolarMaths.SectorAngle(startPos.r, IslandGenerator.wallWidth * widthMultiplier)); //Mathf.Acos(1 - (IslandGenerator.wallWidth * widthMultiplier) / (2 * startPos.r * startPos.r)); // cosine rule
                                                                                                                        // or: = PolarMaths.SectorAngle(startPos.r, IslandGenerator.wallWidth * widthMultiplier); 
                                                                                                                        // Probably close enough to be correct, but as sector angle uses an arc length, it could cause problems with small circles

        return startPos + (numWalls / 2) * changePerWall;
    }

    public void CreateWallSegment(NeighbourInfo neighbour, ref List<GameObject> allObjects)
    {
        float fullWallLength, unroundedNumWalls, widthMultiplier;
        Polar changePerWall = new();
        int numWalls;
        GameObject gateObj, wallObj;
        Polar startPos;
        bool isSideways = neighbour.direction == Directions.Right || neighbour.direction == Directions.Left;

        if (!isSideways)
        {
            float fullAngle = PolarMaths.AngleBetween(bottomRight.Theta, neighbour.neighbour.bottomRight.Theta) + PolarMaths.AngleBetween(neighbour.neighbour.bottomRight.Theta, topLeft.Theta);
            bool rightInside = Mathf.Approximately(fullAngle, Width) && fullAngle <= 2 * Mathf.PI;
            fullAngle = PolarMaths.AngleBetween(bottomRight.Theta, neighbour.neighbour.topLeft.Theta) + PolarMaths.AngleBetween(neighbour.neighbour.topLeft.Theta, topLeft.Theta);
            bool leftInside = Mathf.Approximately(fullAngle, Width) && fullAngle <= 2 * Mathf.PI;
            float rightTheta = rightInside ? neighbour.neighbour.bottomRight.Theta : bottomRight.Theta;
            float leftTheta = leftInside ? neighbour.neighbour.topLeft.Theta : topLeft.Theta;
            startPos = new(
            neighbour.direction == Directions.Up ? topLeft.r : bottomRight.r,
            rightTheta
            );

            gateObj = ObjectReferences.Instance.gate1;
            wallObj = ObjectReferences.Instance.wall1;

            fullWallLength = PolarMaths.ArcLength(startPos.r, PolarMaths.AngleBetween(rightTheta, leftTheta));
            unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
            numWalls = Mathf.RoundToInt(unroundedNumWalls);
            widthMultiplier = unroundedNumWalls / numWalls;
            changePerWall.Theta = PolarMaths.SectorAngle(startPos.r, IslandGenerator.wallWidth * widthMultiplier); //Mathf.Acos(1 - (IslandGenerator.wallWidth * widthMultiplier) / (2 * startPos.r * startPos.r)); // cosine rule
                // or: = PolarMaths.SectorAngle(startPos.r, IslandGenerator.wallWidth * widthMultiplier); 
                // Probably close enough to be correct, but as sector angle uses an arc length, it could cause problems with small circles
        }
        else
        {
            startPos = new(bottomRight.r,
            neighbour.direction == Directions.Left ? topLeft.Theta : bottomRight.Theta
            );

            gateObj = ObjectReferences.Instance.sideGate1;
            wallObj = ObjectReferences.Instance.sideWall1;

            fullWallLength = Height;
            unroundedNumWalls = fullWallLength / IslandGenerator.wallWidth;
            numWalls = Mathf.RoundToInt(unroundedNumWalls);
            widthMultiplier = unroundedNumWalls / numWalls;
            changePerWall.r = IslandGenerator.wallWidth * widthMultiplier;
        }

        Polar currentPos = startPos + changePerWall / 2f ;
        Gate gate = null;
        List<GameObject> walls = new();

        for (int i = 0; i < numWalls; i++)
        {
            if (i == numWalls / 2) // gate index
            {
                allObjects.Add(Object.Instantiate(gateObj, PolarMaths.P2V3(currentPos),
                        Quaternion.Euler(0, currentPos.ThetaDegreesClockwise, 0), LevelController.Instance.wallsNGates));
                gate = allObjects[^1].GetComponent<Gate>();
                gate.isSideways = isSideways;
            }
            else
            {
                walls.Add(Object.Instantiate(wallObj, PolarMaths.P2V3(currentPos),
                    Quaternion.Euler(0, currentPos.ThetaDegreesClockwise, 0), LevelController.Instance.wallsNGates));
            }
            currentPos += changePerWall;
        }

        foreach (GameObject wall in walls)
        {
            wall.GetComponent<Wall>().parent = gate;
        }
        neighbour.gate = gate;
        gate.linkedWalls = walls;
    }

    public void BuildZone()
    {
        List<GameObject> allObjects = new();
        for (int i = 0; i < neighbourInfos.Count; i++)
        {
            NeighbourInfo info = neighbourInfos[i];
            if (info.direction == Directions.Down || info.gate != null) continue;

            CreateWallSegment(info, ref allObjects);

            //setup the gate's connections
            info.gate.connectionInfo.zone1 = this;
            info.gate.connectionInfo.zone2 = info.neighbour;
            info.gate.connectionInfo.index1 = i;
            info.gate.connectionInfo.index2 = info.neighbour.neighbourInfos.FindIndex(g => g.neighbour == this);

            // tell the neighbouring zone about the gate
            info.neighbour.neighbourInfos[info.gate.connectionInfo.index2].gate = info.gate;
        }

        LevelController.CreateJob(allObjects.ToArray(), 2, (int) Gate.WallUpgradeHits.level1, false);

        // remove the mounds
        foreach (DirtMound mound in mounds)
            Object.Destroy(mound.gameObject);
        mounds = null;
    }

    public void UpgradeWalls(bool ignoreRepairCheck = false)
    {
        if (!ignoreRepairCheck && neighbourInfos.Where(info => info.gate.isDestroyed && info.direction != Directions.Down).Count() != 0)
        {
            RepairWalls();
        }

        foreach (var n in neighbourInfos.Where(g => g.direction == Directions.Up)) n.gate.RebuildWalls(false);
        foreach (var n in neighbourInfos.Where(g => g.direction == Directions.Right)) n.gate.RebuildWalls(false);
        foreach (var n in neighbourInfos.Where(g => g.direction == Directions.Left)) n.gate.RebuildWalls(false);
    }

    public void RepairWalls()
    {
        foreach (var n in neighbourInfos.Where(g => g.direction == Directions.Up)) n.gate.RebuildWalls(true);
        foreach (var n in neighbourInfos.Where(g => g.direction == Directions.Right)) n.gate.RebuildWalls(true);
        foreach (var n in neighbourInfos.Where(g => g.direction == Directions.Left)) n.gate.RebuildWalls(true);
    }

    public Vector3 GetRandomEmptyPoint(Vector2 spaceRequired = new())
    {
        const int maxIterations = 50;
        Vector3 actualSpaceRequired = new Vector3(spaceRequired.x < 1 ? 1 : spaceRequired.x, 0, spaceRequired.x < 1 ? 1 : spaceRequired.x) / 2f;
        for (int i = 0; i < maxIterations; i++)
        {
            var pointToCheck = PolarMaths.P2V3(new Polar(Random.Range(bottomRight.r, topLeft.r), Random.Range(bottomRight.Theta, topLeft.Theta)));

            // should check if something is there, add param spaceRequired for building check
            if (!Physics.CheckBox(pointToCheck, actualSpaceRequired, Quaternion.identity, GameController.Instance.obstacleMask))
                return pointToCheck;
        }
        return Vector3.zero;
    }
}
