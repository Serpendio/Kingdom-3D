using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    class TimeEvent
    {
        public float activateTime;
        public event Action OnEvent;

        public void InvokeEvent()
        {
            OnEvent.Invoke();
        }

        public int NumSubscribedClients => OnEvent?.GetInvocationList().Length ?? 0;

        public TimeEvent(float activateTime, Action firstAction)
        {
            this.activateTime = activateTime;
            OnEvent += firstAction;
        }
    }

    private static float _currentTime;// = sunrise;
    public static int DayNum { get; private set; }
    public static float CurrentTime { get { return _currentTime; } set { _currentTime = value; } }

    //public const float dayLength = 1440;
    //public const float timeConversionRate = 360; // 1 day = 1440 mins = 86400 = 4 irl mins, i.e. every minute = six in game hours
    public const float dawn = 35; // 3:30, day starts, bell can toll after last greed is defeated saving the game and letting subjects wander
                            // greedlings from portals will now retreat however all others must be defeated
    public const float sunrise = 55; // 5:30
    public const float noon = 125; // 12:30, when active portals can begin to recognise hostile presences and release defense waves
    public const float sunset = 200; // 20:00, when the kingdom retreats behind walls
    public const float midnight = 240; // 00:00 nightly waves are meant to reach the outermost walls at this time

    private static readonly List<TimeEvent> _events = new List<TimeEvent>();

    void UpdateTime()
    {
        float newTime = _currentTime + Time.deltaTime;
        if (newTime > midnight) newTime -= midnight;

        foreach (var e in _events)
        {
            if (newTime > _currentTime)
            { 
                if (_currentTime < e.activateTime && e.activateTime <= newTime) 
                { 
                    e.InvokeEvent();
                }
            }
            else if (newTime < _currentTime)
            {
                if (_currentTime < e.activateTime && e.activateTime <= midnight || 0 < e.activateTime && e.activateTime <= newTime)
                {
                    e.InvokeEvent();
                }
            }
        }

        CurrentTime = newTime;
    }

    private void Update()
    {
        UpdateTime();
    }

    public static void AddActionAtTime(float time, Action functionToCall)
    {
        if (time <= 0 || time > midnight)
        {
            throw new ArgumentOutOfRangeException("time");
        }

        if (_events.Count == 0)
        {
            _events.Add(new TimeEvent(time, functionToCall));
            return;
        }

        for (int i = 0; i < _events.Count; i++)
        {
            if (Mathf.Approximately(_events[i].activateTime, time))
            {
                _events[i].OnEvent += functionToCall;
                return;
            }
            else if (_events[i].activateTime > time)
            {
                _events.Insert(i, new TimeEvent(time, functionToCall));
                return;
            }
        }

        _events.Add(new TimeEvent(time, functionToCall));
        return;
    }

    public static void RemoveActionFromTime(float time, Action functionToRemove)
    {
        for (int i = 0; i < _events.Count; i++)
        {
            if (Mathf.Approximately(_events[i].activateTime, time))
            {
                _events[i].OnEvent -= functionToRemove;
                if (_events[i].NumSubscribedClients == 0)
                {
                    _events.RemoveAt(i);
                }
                return;
            }
        }

        throw new KeyNotFoundException("No actions exist for this time");
    }
}
