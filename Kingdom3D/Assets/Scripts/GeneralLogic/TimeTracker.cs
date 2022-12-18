using System;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    public static int DaysPast { get; private set; }
    public static float CurrentTime { get; private set; } = sunrise;

    public const float dayLength = 1440;
    public const float timeConversionRate = 360; // 1 day = 1440 mins = 4 irl mins, i.e. every minute = six in game hours
    public const float dawn = 210; // 3:30, day starts, bell can toll after last greed is defeated saving the game and letting subjects wander
                            // greedlings from portals will now retreat however all others must be defeated
    public const float sunrise = 330; // 5:30
    public const float noon = 750; // 12:30, when active portals can begin to recognise hostile presences and release defense waves
    public const float sunset = 1200; // 20:00, when the kingdom retreats behind walls
    public const float midnight = 1440; // 00:00 nightly waves are meant to reach the outermost walls at this time

    public static event Action OnDawnPassed = delegate { };
    public static event Action OnSunrisePassed = delegate { };
    public static event Action OnNoonPassed = delegate { };
    public static event Action OnSunsetPassed = delegate { };
    public static event Action OnMidnightPassed = delegate { };

    void UpdateTime()
    {
        float newTime = CurrentTime + Time.deltaTime * timeConversionRate / 60f;

        if (CurrentTime < dawn && dawn <= newTime)
            OnDawnPassed.Invoke();
        else if (CurrentTime < sunrise && sunrise <= newTime)
            OnSunrisePassed.Invoke();
        else if (CurrentTime < noon && noon <= newTime)
            OnNoonPassed.Invoke();
        else if (CurrentTime < sunset && sunset <= newTime)
            OnSunsetPassed.Invoke();
        else if (CurrentTime < midnight && midnight <= newTime)
            OnMidnightPassed.Invoke();

        CurrentTime = newTime;
        if (CurrentTime > midnight) CurrentTime -= midnight;
    }

    private void Update()
    {
        UpdateTime();
    }
}
