using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarPosVis : MonoBehaviour
{
    [SerializeField] Polar polar;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(PolarMaths.P2V3(polar), 1);
    }
}
