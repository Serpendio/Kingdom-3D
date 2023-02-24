using System;
using System.Collections.Generic;
using UnityEngine;

public class SafetyCheck
{
    public static event Action OnKingdomSafe = delegate { };
    public static event Action OnKingdomNotSafe = delegate { };
    public static event Action<List<Zone>> OnWallStatusChange = delegate { };
    public static bool isKingdomSafe;

    public SafetyCheck()
    {
        TimeTracker.OnDawnPassed += () => { if (GreedTracker.allKilled) OnKingdomSafe(); };
    }

    public static void MakeSafe()
    {
        OnKingdomSafe.Invoke();
        isKingdomSafe = true;
    }

    public static void MakeNotSafe()
    {
        OnKingdomNotSafe.Invoke();
        isKingdomSafe = false;
    }
}
