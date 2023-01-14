using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGenerator : MonoBehaviour
{

    [Header("Zone Settings")]
    [SerializeField, Min(0)] float minZoneArc = 90;
    [SerializeField, Min(0)] float maxZoneArc = 120;
    [SerializeField, Min(0)] float minZoneRadius = 20;
    [SerializeField, Min(0)] float maxZoneRadius = 30;

    [Header("Set Variables")]
    public const float wallWidth = 2;
    [SerializeField] float[] presetRingRadii = new float[] { 20, 40 };
    [SerializeField] int[] presetNumRingZones = new int[] { 2, 3 };

    [Header("Per Island Variables")]
    [SerializeField, Min(100)] float islandRadius = 400f;

    [Header("Debug")]
    [SerializeField, Min(-1)] int seed = -1;

    [ContextMenu("Setup World")]
    private void Awake()
    {
        Random.InitState(seed == -1 ? Random.Range(0, int.MaxValue) : seed);

        CreateZones();
    }

    public void CreateZones() // could create using a bsp
    {
        float radiusUsed = 0;
        float PI2 = Mathf.PI * 2;

        List<List<float>> allAngles = new(); // each list represents one ring, each list contains every zone split, 0 & 2Pi included
        List<float> radii = new() { 0 };
        List<Zone> zones = new();

        // add the preset values
        if (presetNumRingZones.Length == presetRingRadii.Length)
        {
            foreach (var radius in presetRingRadii)
            {
                radiusUsed += radius;
                radii.Add(radiusUsed);
            }
            foreach (var numZones in presetNumRingZones)
            {
                float angleUsed = 0;
                List<float> angles = new() { 0 };
                for (int i = 0; i < numZones; i++)
                {
                    angleUsed += PI2 / numZones;
                    angles.Add(angleUsed);
                }
                allAngles.Add(angles);
            }
        }
        else Debug.LogWarning("WARN: Generator has unequal preset lengths. Presets will be skipped");

        // add the random values
        while (islandRadius - radiusUsed > 1.5f * maxZoneRadius)
        {
            radiusUsed += Random.Range(minZoneRadius, maxZoneRadius); ;
            radii.Add(radiusUsed);

            float minAngle = PolarMaths.SectorAngle(radiusUsed, minZoneArc);
            float maxAngle = PolarMaths.SectorAngle(radiusUsed, maxZoneArc);

            float angleUsed = 0;
            List<float> angles = new() { 0 };

            while (PI2 - angleUsed > 1.5f * maxAngle)
            {
                angleUsed += Random.Range(minAngle, maxAngle);
                angles.Add(angleUsed);
            }

            if (PI2 - angleUsed > maxAngle)
            {
                angleUsed += Random.Range(minAngle, maxAngle);
                angles.Add(angleUsed);
            }
            else if (PI2 - angleUsed > minAngle)
            {
                angleUsed += Random.Range(minAngle, PI2 - angleUsed);
                angles.Add(angleUsed);
            }
            
            for (int i = 1; i < angles.Count - 1; i++)
            {
                angles[i] += (PI2 - angleUsed) / (angles.Count - 2);
            }
            angles[^1] = PI2;

            allAngles.Add(angles);
        }

        // final non-buildable layer enclosing everything
        radii.Add(islandRadius - radiusUsed);
        allAngles.Add(new List<float>() { 0, PI2 });

        for (int r = 0; r < radii.Count - 1; r++)
        {
            for (int a = 0; a < allAngles[r].Count - 1; a++)
            {
                zones.Add(new Zone(new Polar(radii[r], allAngles[r][a]),
                                   new Polar(radii[r + 1], allAngles[r][a + 1])));
                /*print(string.Format("Top Left: {1}, Bottom Right: {0}",
                    new Polar(radii[r], allAngles[r][a]).ToRoundedStringDegrees(0, 0),
                    new Polar(radii[r + 1], allAngles[r][a + 1]).ToRoundedStringDegrees(0, 0)));*/
            }
        }

        for (int i = 0; i < allAngles[0].Count; i++)
        {
            zones[i].isCentralZone = true;
        }

        
        // terribly unoptimised
        for (int i = 0; i < zones.Count; i++)
        {
            for (int o = 0; o < zones.Count; o++)
            {
                if (i == o) continue;

                if (ZoneYBordersX(zones[i], zones[o], out Directions direction))
                {
                    zones[i].neighbouringZones.Add(zones[o]);
                    zones[i].gateDirections.Add(direction);
                }
            }

            zones[i].gates = new Gate[zones[i].neighbouringZones.Count];
        }

        for (int i = 0; i < zones.Count; i++)
        {
            zones[i].PlaceMounds();
        }


        //print(string.Format("Created {0} zones", zones.Count));
        LevelController.Instance.zones = zones;
    }

    static bool ZoneYBordersX(Zone x, Zone y, out Directions direction)
    {
        // if bsp is used, will need three more but with theta and r flipped (with slight adjustments), will need to change Zone.GetGatePos too

        if (Mathf.Approximately(x.topLeft.r, y.bottomRight.r)) // is row above
        {
            direction = Directions.Up;
            return (x.topLeft.theta - y.bottomRight.theta > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 1.5f) || // gap > 1.5 * wallWidth left or
                    y.topLeft.theta - x.bottomRight.theta > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 1.5f));   // gap > 1.5 * wallWidth right
        }
        else if (Mathf.Approximately(x.bottomRight.r, y.topLeft.r))
        {
            direction = Directions.Down;
            return (x.topLeft.theta - y.bottomRight.theta > PolarMaths.SectorAngle(x.bottomRight.r, wallWidth * 1.5f) || // gap > 1.5 * wallWidth left or
                    y.topLeft.theta - x.bottomRight.theta > PolarMaths.SectorAngle(x.bottomRight.r, wallWidth * 1.5f));   // gap > 1.5 * wallWidth right
        }
        else if (x.topLeft.r == y.topLeft.r)
        {
            if (Mathf.Approximately(x.topLeft.theta - y.bottomRight.theta, 0))
            {
                direction = Directions.Left;
                return true;
            }
            else if (Mathf.Approximately(y.topLeft.theta - x.bottomRight.theta, 0))
            {
                direction = Directions.Right;
                return true;
            }
        }
        
        direction = Directions.Down;
        return false;

        /*return (
            Mathf.Approximately(x.topLeft.r, y.bottomRight.r) && // row above and
            (x.topLeft.theta - y.bottomRight.theta > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 1.5f) || // there is a gap of more than 1.5 * wallWidth to the left or
            y.topLeft.theta - x.bottomRight.theta > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 1.5f)) // there is a gap of more than 1.5 * wallWidth to the right
            || // or...
            Mathf.Approximately(x.bottomRight.r, y.topLeft.r) && // row below and
            (x.topLeft.theta - y.bottomRight.theta > PolarMaths.SectorAngle(x.bottomRight.r, wallWidth * 1.5f) || // there is a gap of more than 1.5 * wallWidth to the left
            y.topLeft.theta - x.bottomRight.theta > PolarMaths.SectorAngle(x.bottomRight.r, wallWidth * 1.5f)) // there is a gap of more than 1.5 * wallWidth to the right
            || // or...
            x.topLeft.r == y.topLeft.r && // same row and
            (Mathf.Approximately(x.topLeft.theta - y.bottomRight.theta, 0) || // there is no gap left or
            Mathf.Approximately(y.topLeft.theta - x.bottomRight.theta, 0)) // there is no gap right

            
        );*/
    }
}
