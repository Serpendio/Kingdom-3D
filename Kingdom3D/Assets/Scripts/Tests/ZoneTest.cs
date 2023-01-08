using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTest : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        foreach (var zone in LevelController.Instance.zones)
        {
            Gizmos.DrawCube(PolarMaths.P2V3((zone.topLeft + zone.bottomRight) / 2), PolarMaths.P2V3(zone.topLeft - zone.bottomRight) + Vector3.up);
        }
    }
}
