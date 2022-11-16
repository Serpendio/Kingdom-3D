using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticFunctions
{
    public static Vector3 Slerp(Vector3 a, Vector3 b, float t, Vector3 center)
    {
        return Vector3.Slerp(a - center, b - center, t) + center;
    }

    public static bool IsLayerInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
