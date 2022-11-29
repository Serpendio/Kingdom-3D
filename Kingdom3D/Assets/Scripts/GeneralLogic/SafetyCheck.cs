using System;
using UnityEngine;

public class SafetyCheck
{
    public static event Action OnKingdomSafe = delegate { };

    public SafetyCheck()
    {
        TimeTracker.OnDawnPassed += () => { if (GreedTracker.allKilled) OnKingdomSafe(); };
    }

    public static void MakeSafe()
    {
        OnKingdomSafe.Invoke();
    }
}
