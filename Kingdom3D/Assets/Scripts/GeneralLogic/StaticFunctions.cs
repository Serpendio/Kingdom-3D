using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticFunctions
{
    public static bool Approximately(this Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
    }

    public static Vector3 Slerp(Vector3 a, Vector3 b, float t, Vector3 center)
    {
        return Vector3.Slerp(a - center, b - center, t) + center;
    }

    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static void ThrowCoin(Vector3 position)
    {
        
    }

    /// <summary>
    /// Returns the top-down displacement between two coordinates
    /// </summary>
    /// <param name="from">Vector3 a</param>
    /// <param name="to">Vector3 b</param>
    /// <returns>Returns (x-dist, z-dist)</returns>
    public static Vector3 BirdsEyeDisplacement(this Vector3 from, Vector3 to)
    {
        return new Vector3(from.x - to.x, 0, from.z - to.z);
    }

    public static Vector3 BirdsEyeDisplacement(this Vector3 to)
    {
        return new Vector3(to.x, 0, to.z);
    }

    /*public static Vector3 Direction2D(Vector3 from, Vector3 to)
    {
        return new Vector3(to.x - from.x, 0, to.z - from.z).normalized;
    }*/
}
