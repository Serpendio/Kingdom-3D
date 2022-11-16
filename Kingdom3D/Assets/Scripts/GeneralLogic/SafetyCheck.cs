using System;
using UnityEngine;

public class SafetyCheck
{
    public static event Action OnKingdomSafe = delegate { };
    public static bool allKilled = false;

    public SafetyCheck()
    {
        TimeTracker.OnDawnPassed += () => { if (allKilled) OnKingdomSafe(); };
    }
}
