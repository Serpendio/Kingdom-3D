using Den.Tools.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIslandGenerator : MonoBehaviour
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
    [SerializeField] GameObject moundUI, zoneUI;
    [SerializeField] float checkZoneMultiplier = 1, neighbouringZoneMultiplier = 1, moundPlaceMultiplier = 1, pauseBetweenMoundsMultiplier = 1;

    [ContextMenu("Setup World")]
    private void Awake()
    {
        if (seed == -1) seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);
        
        StartCoroutine(CreateZones());
    }

    public IEnumerator CreateZones() // could create using bsp
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
        while (islandRadius - radiusUsed > 1.5f * maxZoneRadius)
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

        #region Extra + Create Zones
        // final non-buildable layer enclosing everything
        radii.Add(islandRadius);
        allAngles.Add(new List<float>() { 0, PI2 });

        for (int r = 0; r < radii.Count - 1; r++)
        {
            for (int a = 0; a < allAngles[r].Count - 1; a++)
            {
                zones.Add(new Zone(new Polar(radii[r], allAngles[r][a]),
                                   new Polar(radii[r + 1], allAngles[r][a + 1])));
            }
        }

        for (int i = 0; i < allAngles[0].Count; i++)
        {
            zones[i].isCentralZone = true;
        }
        #endregion

        #region Setup Gates
        GameObject[] UIZones = new GameObject[zones.Count];
        GameObject[][] UIMounds = new GameObject[zones.Count - 1][];

        for (int i = 0; i < zones.Count; i++)
        {
            var ui = Instantiate(zoneUI, transform);
            ui.GetComponent<RectTransform>().sizeDelta = 2 * zones[i].bottomRight.r * Vector2.one;
            ui.GetComponent<ZoneUI>().bottomRight = zones[i].bottomRight;
            ui.GetComponent<ZoneUI>().topLeft = zones[i].topLeft;
            ui.GetComponent<ZoneUI>().gateDirections = zones[i].gateDirections.ToArray();
            ui.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = 2 * zones[i].topLeft.r * Vector2.one;
            ui.transform.GetChild(0).GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, zones[i].bottomRight.ThetaDegrees);
            ui.transform.GetChild(0).GetComponent<Image>().fillAmount = zones[i].Width / PI2;
            ui.transform.GetChild(0).GetComponent<Image>().color = new Color(Random.Range(0.7f, 1f), Random.Range(0, 0.3f), Random.Range(0, 0.3f));
            UIZones[i] = ui;
        }

        for (int i = 0; i < zones.Count - 1; i++)
        {
            for (int o = i + 1; o < zones.Count; o++)
            {
                UIZones[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, .6f, 0f);
                UIZones[o].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, .6f, 0f);
                yield return new WaitForSeconds(0.005f * checkZoneMultiplier);

                if (IslandGenerator.ZoneYBordersX(zones[i], zones[o], out Directions direction) && !(i < presetNumRingZones[0] && o < presetNumRingZones[0]))
                {
                    zones[i].neighbouringZones.Add(zones[o]);
                    zones[i].gateDirections.Add(direction);
                    zones[o].neighbouringZones.Add(zones[i]);
                    zones[o].gateDirections.Add((Directions)(((int)direction + 2) % 4));

                    UIZones[i].transform.GetChild(0).GetComponent<Image>().color = new Color(0, 1f, 0);
                    UIZones[o].transform.GetChild(0).GetComponent<Image>().color = new Color(0, 1f, 0);
                    yield return new WaitForSeconds(.05f * neighbouringZoneMultiplier);
                }

                UIZones[i].transform.GetChild(0).GetComponent<Image>().color = new Color(Random.Range(0.7f, 1f), Random.Range(0, 0.3f), Random.Range(0, 0.3f));
                UIZones[o].transform.GetChild(0).GetComponent<Image>().color = new Color(Random.Range(0.7f, 1f), Random.Range(0, 0.3f), Random.Range(0, 0.3f));
            }

            zones[i].gates = new Gate[zones[i].neighbouringZones.Count];
            UIZones[i].GetComponent<ZoneUI>().gateDirections = zones[i].gateDirections.ToArray();
        }
        #endregion

        #region UI

        for (int i = 0; i < zones.Count - 1; i++) // -1 as we don't want to include the last zone
        {
            UIZones[i].transform.GetChild(0).GetComponent<Image>().color = Color.green;
            yield return new WaitForSeconds(0.1f * moundPlaceMultiplier);
            UIMounds[i] = new GameObject[zones[i].neighbouringZones.Count];
            for (int o = 0; o < zones[i].neighbouringZones.Count; o++)
            {
                var pos = zones[i].GetGatePos(zones[i].neighbouringZones[o], zones[i].gateDirections[o]);
                UIMounds[i][o] = Instantiate(moundUI, transform);
                UIMounds[i][o].GetComponent<RectTransform>().localPosition = PolarMaths.P2V2(pos);
                yield return new WaitForSeconds(0.1f * moundPlaceMultiplier);
            }
            yield return new WaitForSeconds(0.3f * pauseBetweenMoundsMultiplier);
            for (int o = 0; o < zones[i].neighbouringZones.Count; o++)
            {
                UIMounds[i][o].GetComponent<Image>().color = Color.gray;
            }
            UIZones[i].transform.GetChild(0).GetComponent<Image>().color = new Color(Random.Range(0.7f, 1f), Random.Range(0, 0.3f), Random.Range(0, 0.3f));
        }
        #endregion

        LevelController.zones = zones.ToArray();
    }
}
