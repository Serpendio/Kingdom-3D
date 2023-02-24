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

    [Header("Debug")]
    [SerializeField, Min(-1)] int seed = -1;

    [ContextMenu("Setup World")]
    private void Awake()
    {
        if (seed == -1) seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);

        CreateZones();
    }

    public void CreateZones() // could create using bsp
    {
        float radiusUsed = 0;
        float rotation;
        float angleUsed;
        float PI2 = Mathf.PI * 2;

        List<List<float>> allAngles = new(); // each list represents one ring, each list contains every zone split, 0 & 2Pi included
        List<float> radii = new() { 0 };
        List<Zone> zones = new();

        #region Preset Values
        if (presetNumRingZones.Length == presetRingRadii.Length)
        {
            foreach (var radius in presetRingRadii)
            {
                radiusUsed += radius;
                radii.Add(radiusUsed);
            }
            foreach (var numZones in presetNumRingZones)
            {
                angleUsed = 0;
                rotation = Random.Range(0, 2 * Mathf.PI);
                List<float> angles = new() { rotation };
                for (int i = 0; i < numZones; i++)
                {
                    angleUsed += PI2 / numZones;
                    angles.Add(angleUsed + rotation);
                }
                allAngles.Add(angles);
            }
        }
        else Debug.LogWarning("WARN: Generator has unequal preset lengths. Presets will be skipped");
        #endregion

        #region Random Values
        while (LevelController.Instance.islandRadius - radiusUsed > 1.5f * maxZoneRadius)
        {
            radiusUsed += Random.Range(minZoneRadius, maxZoneRadius); ;
            radii.Add(radiusUsed);

            float minAngle = PolarMaths.SectorAngle(radiusUsed, minZoneArc);
            float maxAngle = PolarMaths.SectorAngle(radiusUsed, maxZoneArc);

            angleUsed = 0;
            rotation = Random.Range(0, 2 * Mathf.PI);
            List<float> angles = new() { rotation };

            while (PI2 - angleUsed > 1.5f * maxAngle)
            {
                angleUsed += Random.Range(minAngle, maxAngle);
                angles.Add(angleUsed + rotation);
            }

            if (PI2 - angleUsed > maxAngle)
            {
                angleUsed += maxAngle;
                angles.Add(angleUsed + rotation);
            }
            if (PI2 - angleUsed > minAngle)
            {
                angleUsed += Random.Range(minAngle, PI2 - angleUsed);
                angles.Add(angleUsed + rotation);
            }

            for (int i = 1; i < angles.Count - 1; i++)
            {
                angles[i] += (PI2 - angleUsed) / (angles.Count - 2);
            }
            angles[^1] = PI2 + rotation;

            allAngles.Add(angles);
        }
        #endregion

        #region Create Zones
        // final non-buildable layer enclosing everything
        radii.Add(LevelController.Instance.islandRadius);
        allAngles.Add(new List<float>() { 0, PI2 });

        int zoneIndex = 0;
        for (int r = 0; r < radii.Count - 1; r++)
        {
            for (int a = 0; a < allAngles[r].Count - 1; a++)
            {
                zones.Add(new Zone(new Polar(radii[r], allAngles[r][a]),
                                   new Polar(radii[r + 1], allAngles[r][a + 1]),
                                   zoneIndex));
                zoneIndex++;
            }
        }

        LevelController.numCentralZones = allAngles[0].Count - 1;
        for (int i = 0; i < allAngles[0].Count - 1; i++) // -1 as start and end should be the exact same point
        {
            zones[i].isCentralZone = true;
        }
        #endregion

        #region Setup Gates

        for (int i = 0; i < zones.Count - 1; i++)
        {
            for (int o = i + 1; o < zones.Count; o++)
            {
                if (ZoneYBordersX(zones[i], zones[o], out Directions direction) && !(i < presetNumRingZones[0] && o < presetNumRingZones[0]))
                {
                    zones[i].neighbourInfos.Add(new(null, direction, zones[o]));
                    zones[o].neighbourInfos.Add(new(null, (Directions)(((int)direction + 2) % 4), zones[i]));
                }
            }

            zones[i].PlaceMounds();
        }
        #endregion

        LevelController.zones = zones.ToArray();
    }

    public static bool ZoneYBordersX(Zone x, Zone y, out Directions direction)
    {
        // if bsp is used, will need to adjust up and down for left and right but with theta and r flipped (with slight adjustments), will need to change Zone.GetGatePos too
        // refer to PossibleGateConnections.png in the git root
        if (Mathf.Approximately(x.topLeft.r, y.bottomRight.r)) // is row above
        {
            direction = Directions.Up;

            return // case 1
                PolarMaths.AngleBetween(y.bottomRight.Theta, x.topLeft.Theta) > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 2.5f) &&
                PolarMaths.AngleBetween(y.topLeft.Theta, x.topLeft.Theta) >= PolarMaths.AngleBetween(y.bottomRight.Theta, x.topLeft.Theta)
            || // case 2
                PolarMaths.AngleBetween(x.bottomRight.Theta, y.topLeft.Theta) > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 2.5f) &&
                PolarMaths.AngleBetween(x.bottomRight.Theta, y.bottomRight.Theta) >= PolarMaths.AngleBetween(x.bottomRight.Theta, y.topLeft.Theta)
            || // case 3
                Mathf.Approximately(PolarMaths.AngleBetween(x.bottomRight.Theta, y.bottomRight.Theta) + PolarMaths.AngleBetween(y.bottomRight.Theta, x.topLeft.Theta), x.Width) &&
                Mathf.Approximately(PolarMaths.AngleBetween(x.bottomRight.Theta, y.topLeft.Theta) + PolarMaths.AngleBetween(y.topLeft.Theta, x.topLeft.Theta), x.Width);
        }
        else if (Mathf.Approximately(x.bottomRight.r, y.topLeft.r))
        {
            direction = Directions.Down;

            return
                PolarMaths.AngleBetween(y.bottomRight.Theta, x.topLeft.Theta) > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 2.5f) &&
                PolarMaths.AngleBetween(x.topLeft.Theta, y.bottomRight.Theta) > PolarMaths.AngleBetween(y.bottomRight.Theta, x.topLeft.Theta)
            ||
                PolarMaths.AngleBetween(x.bottomRight.Theta, y.topLeft.Theta) > PolarMaths.SectorAngle(x.topLeft.r, wallWidth * 2.5f) &&
                PolarMaths.AngleBetween(y.topLeft.Theta, x.bottomRight.Theta) > PolarMaths.AngleBetween(x.bottomRight.Theta, y.topLeft.Theta)
            ||
                Mathf.Approximately(PolarMaths.AngleBetween(y.bottomRight.Theta, x.bottomRight.Theta) + PolarMaths.AngleBetween(x.bottomRight.Theta, y.topLeft.Theta), y.Width) &&
                Mathf.Approximately(PolarMaths.AngleBetween(y.bottomRight.Theta, x.topLeft.Theta) + PolarMaths.AngleBetween(x.topLeft.Theta, y.topLeft.Theta), y.Width);
        }
        else if (x.topLeft.r == y.topLeft.r)
        {
            if (Mathf.Approximately(x.topLeft.Theta, y.bottomRight.Theta))
            {
                direction = Directions.Left;
                return true;
            }
            else if (Mathf.Approximately(y.topLeft.Theta, x.bottomRight.Theta))
            {
                direction = Directions.Right;
                return true;
            }
        }

        direction = Directions.Down;
        return false;
    }
}
