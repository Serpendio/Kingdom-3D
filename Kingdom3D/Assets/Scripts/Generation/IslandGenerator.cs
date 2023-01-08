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

        // terribly unoptimised
        for (int i = 0; i < zones.Count; i++)
        {
            for (int o = 0; o < zones.Count; o++)
            {
                if (i == o) continue;

                if (ZoneYBordersX(zones[i], zones[o]))
                {
                    zones[i].neighbouringZones.Add(zones[o]);
                }
            }
        }

        //print(string.Format("Created {0} zones", zones.Count));
        LevelController.Instance.zones = zones;
    }

    static bool ZoneYBordersX(Zone x, Zone y)
    {
        return (x.topLeft.r == y.bottomRight.r ||   // (y is in the ring above x    or
            x.bottomRight.r == y.topLeft.r ||       //  y is in the same ring as x  or 
            x.topLeft.r == y.topLeft.r) &&          //  y is in the ring below x)   and
            ((y.topLeft.theta >= x.topLeft.theta && y.bottomRight.theta <= x.topLeft.theta) ||         // x left side is between two y sides    or
                y.topLeft.theta >= x.bottomRight.theta && y.bottomRight.theta <= x.bottomRight.theta); // x right side is between two y sides
    }
}
