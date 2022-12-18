using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    public static void ThrowCoin(Vector3 position)
    {
        
    }

    /// <summary>
    /// Returns the top-down displacement between two transforms
    /// </summary>
    /// <param name="from">Transform a</param>
    /// <param name="to">Transform b</param>
    /// <returns>Returns (x-dist, z-dist)</returns>
    public static Vector3 BirdsEyeDisplacement(Transform from, Transform to)
    {
        return new Vector3(from.position.x - to.position.x, 0, from.position.z - to.position.z);
    }

    /*public static Vector3 Direction2D(Vector3 from, Vector3 to)
    {
        return new Vector3(to.x - from.x, 0, to.z - from.z).normalized;
    }*/
}
