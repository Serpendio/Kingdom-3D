using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Polar
{
    public float r;
    public float theta; // in radians

    public float ThetaDegrees => theta * Mathf.Rad2Deg;

    public Polar(float r, float theta)
    {
        this.r = r;
        this.theta = theta;
    }

    public static Polar operator +(Polar a, Polar b)
    {
        return new Polar(a.r + b.r, a.theta + b.theta);
    }

    public static Polar operator -(Polar a, Polar b)
    {
        return new Polar(a.r - b.r, a.theta - b.theta);
    }

    public static Polar operator /(Polar a, float b)
    {
        return new Polar(a.r / b, a.theta / b);
    }

    public static Polar operator *(Polar a, float b)
    {
        return new Polar(a.r * b, a.theta * b);
    }

    public static Polar operator *(float a, Polar b)
    {
        return new Polar(a * b.r, a * b.theta);
    }

    public override string ToString()
    {
        return string.Format("(r: {0}, theta: {1})", r, theta);
    }

    public string ToRoundedString(int rDecimals, int thetaDecimals)
    {
        var multiplyValue1 = Mathf.RoundToInt(Mathf.Pow(10, rDecimals));
        var multiplyValue2 = Mathf.RoundToInt(Mathf.Pow(10, thetaDecimals));
        return string.Format("(r: {0}, theta: {1})", Mathf.Round(r * multiplyValue1) / multiplyValue1, Mathf.Round(theta * multiplyValue2) / multiplyValue2);
    }

    public string ToRoundedStringDegrees(int rDecimals, int degreeDecimals)
    {
        var multiplyValue1 = Mathf.RoundToInt(Mathf.Pow(10, rDecimals));
        var multiplyValue2 = Mathf.RoundToInt(Mathf.Pow(10, degreeDecimals));
        return string.Format("(r: {0}, theta: {1})", Mathf.Round(r * multiplyValue1) / multiplyValue1, Mathf.Round(ThetaDegrees * multiplyValue2) / multiplyValue2);
    }
}

public static class PolarMaths
{
    public static Polar CartesianToPolar(float x, float z)
    {
        return new Polar(Mathf.Sqrt(x*x + z*z), Mathf.Atan2(z, x));
    }

    public static Polar CartesianToPolar(Vector2 coord)
    {
        return CartesianToPolar(coord.x, coord.y);
    }

    public static Polar CartesianToPolar(Vector3 coord)
    {
        return CartesianToPolar(coord.x, coord.z);
    }

    public static Vector2 PolarToCartesian(Polar coord)
    {
        return new Vector2(coord.r * Mathf.Cos(coord.theta), coord.r * Mathf.Sin(coord.theta));
    }

    public static Vector2 P2V2(Polar coord)
    {
        return new Vector2(coord.r * Mathf.Cos(coord.theta), coord.r * Mathf.Sin(coord.theta));
    }

    public static Vector3 P2V3(Polar coord)
    {
        return new Vector3(coord.r * Mathf.Cos(coord.theta), 0, coord.r * Mathf.Sin(coord.theta));
    }

    public static float ArcLength(Polar polar)
    {
        return polar.r * polar.theta;
    }

    public static float ArcLength(float r, float theta)
    {
        return r * theta;
    }

    public static float SectorArea(float r, float theta)
    {
        return r * r * theta / 2;
    }

    public static float SectorAreaFromArc(float r, float arcLength)
    {
        return r * arcLength / 2;
    }

    public static float SectorAngle(float r, float arcLength)
    {
        return arcLength / r;
    }
}
