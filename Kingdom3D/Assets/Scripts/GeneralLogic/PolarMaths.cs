using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Polar
{
    public float r;
    private float theta; // in radians anticlockwise

    public float Theta { 
        get { return theta; } 
        set { theta = value < 0 ?
                Mathf.PI * 2 + value % (Mathf.PI * 2) :
                Mathf.Approximately(value, Mathf.PI * 2) ? 0 :
                value > Mathf.PI * 2 ? value % (Mathf.PI * 2) : 
                value;
        } 
    } 

    public float ThetaDegrees => Theta * Mathf.Rad2Deg;
    public float ThetaDegreesClockwise => 360 - Theta * Mathf.Rad2Deg;

    public Polar(float r, float theta)
    {
        this.r = r;
        this.theta = theta < 0 ?
                Mathf.PI * 2 + theta % (Mathf.PI * 2) :
                Mathf.Approximately(theta, Mathf.PI * 2) ? 0 :
                theta > Mathf.PI * 2 ? theta % (Mathf.PI * 2) :
                theta;
    }

    public static Polar operator +(Polar a, Polar b)
    {
        return new Polar(a.r + b.r, a.Theta + b.Theta);
    }

    public static Polar operator -(Polar a, Polar b)
    {
        return new Polar(a.r - b.r, a.Theta - b.Theta);
    }

    public static Polar operator /(Polar a, float b)
    {
        return new Polar(a.r / b, a.Theta / b);
    }

    public static Polar operator *(Polar a, float b)
    {
        return new Polar(a.r * b, a.Theta * b);
    }

    public static Polar operator *(float a, Polar b)
    {
        return new Polar(a * b.r, a * b.Theta);
    }

    public override string ToString()
    {
        return string.Format("(r: {0}, theta: {1})", r, Theta);
    }

    public string ToRoundedString(int rDecimals, int thetaDecimals)
    {
        var multiplyValue1 = Mathf.RoundToInt(Mathf.Pow(10, rDecimals));
        var multiplyValue2 = Mathf.RoundToInt(Mathf.Pow(10, thetaDecimals));
        return string.Format("(r: {0}, theta: {1})", Mathf.Round(r * multiplyValue1) / multiplyValue1, Mathf.Round(Theta * multiplyValue2) / multiplyValue2);
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
        return new Vector2(coord.r * Mathf.Cos(coord.Theta), coord.r * Mathf.Sin(coord.Theta));
    }

    public static Vector2 P2V2(Polar coord)
    {
        return new Vector2(coord.r * Mathf.Cos(coord.Theta), coord.r * Mathf.Sin(coord.Theta));
    }

    public static Vector3 P2V3(Polar coord)
    {
        return new Vector3(coord.r * Mathf.Cos(coord.Theta), 0, coord.r * Mathf.Sin(coord.Theta));
    }

    public static float ArcLength(Polar polar)
    {
        return polar.r * polar.Theta;
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

    /// <summary>
    /// returns the anticlockwise angle from theta1 to theta2
    /// </summary>
    public static float AngleBetween(float theta1, float theta2)
    {
        if (theta1 < theta2)
            return theta2 - theta1;
        else
            return 2 * Mathf.PI - (theta1 - theta2);
    }


    /// <summary>
    /// returns the anticlockwise centre angle from theta1 to theta2
    /// </summary>
    public static float CentreAngle(float theta1, float theta2)
    {
        if (theta1 <= theta2)
            return (theta1 + theta2) / 2;
        else
            return ((2 * Mathf.PI + theta1 + theta2) / 2f) % (2 * Mathf.PI);
    }
}
