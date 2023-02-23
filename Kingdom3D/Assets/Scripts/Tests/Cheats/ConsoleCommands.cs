using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Contains each command to be entered into the console.
/// The class must inherit from monobehaviour
/// Each command must be public
/// They must return bool returning true if the command succeded else false
/// The first parameter must be a string reference type. This if set will be output to the log
/// The command will be split into subsections split by the underscore
/// If multiple conflicting length commands exist, those with less parameters will be chosen first. If the arguments fail to cast, the next will be called, etc.
/// E.g. "time(bool show, bool analogue)" will be called after "time_set(int time)" which will be called after "time_set_day()"
/// 
/// An example command is:
/// public static bool test(ref string message, int a)
/// {
///    do something
///    if (problem)
///    {
///        message = "warning message"
///        return false
///    }
///    return true;
/// }
/// </summary>
public class ConsoleCommands : MonoBehaviour
{
#if UNITY_EDITOR
    private void Awake()
    {
        string msg = "";
        gamecheat_setinstantbuild(ref msg, true);
        ui_toggleminimap(ref msg);
        //ui_showmap(ref msg);
    }
#endif

    public bool ui_showmap(ref string _)
    {
        Map.Instance.EnableMap();
        return true;
    }

    public bool ui_toggleminimap(ref string _)
    {
        Map.Instance.ToggleMinimap();
        return true;
    }

    public static bool time_scale_set(ref string message, float timeScale)
    {
        if (timeScale > 0 && timeScale < 10)
        {
            Time.timeScale = timeScale;
            return true;
        }
        else
        {
            message = "time scale must be between 0 and 10";
            return false;
        }
    }

    public static bool time_scale_get(ref string message)
    {
        message = Time.timeScale.ToString("n2");
        return true;
    }

    public static bool gamecheat_setinstantbuild(ref string message, bool instantBuild)
    {
        if (instantBuild)
        {
            while (LevelController.GetJob(out BuildJob job))
            {
                typeof(BuildJob).GetMethod("CompleteUpgrade").Invoke(job, null);
            }
        }
        CheatSettings.instantBuild = instantBuild;

        return true;
    }

    public static bool zone_upgrade(ref string message)
    {
        var pos = PolarMaths.CartesianToPolar(LevelController.player.position);
        var zones = LevelController.zones;
        for (int i = 0; i < zones.Length - 1; i++)
        {
            if (zones[i].ContainsPoint(pos))
            {
                var gate = zones[i].neighbourInfos.Where(n => n.direction == Directions.Up).FirstOrDefault().gate; //zones[i].gates[zones[i].gateDirections.IndexOf(Directions.Up)].level;
                
                if (gate == null)
                {
                    zones[i].BuildZone();
                    return true;
                }

                if (gate.level == Gate.WallLevels.level4)
                {
                    message = "zone is already at max level";
                    return false;
                }

                zones[i].UpgradeWalls(false);
                return true;
            }
        }
        message = "player is not currently in a zone";
        return false;
    }

    public static bool zone_repair(ref string message)
    {
        var pos = PolarMaths.CartesianToPolar(LevelController.player.position);
        var zones = LevelController.zones;
        for (int i = 0; i < zones.Length - 1; i++)
        {
            if (zones[i].ContainsPoint(pos))
            {

                zones[i].RepairWalls();
                return true;
            }
        }
        message = "player is not currently in a zone";
        return false;
    }

    /*public static bool time_minute_get(ref string message)
    {
        message = Time.timeScale.ToString("n2");
        return true;
    }

    public static bool time_minute_set(ref string message, int minute)
    {
        if (0 < minute && minute < TimeTracker.dayLength)
        {
            typeof(TimeTracker)
                .GetField(nameof(TimeTracker.CurrentTime), BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(null, minute);
            return true;
        }
        else
        {
            message = string.Format("time scale must be between 0 and {0}", TimeTracker.dayLength);
            return false;
        }
    }

    public static bool time_day_set(ref string message, int day)
    {
        if (day > 0)
        {
            TimeTracker.DayNum = day;
            return true;
        }
        else
        {
            message = "time scale must be between 0 and 10";
            return false;
        }
    }

    public static bool time_day_get(ref string message)
    {
        message = TimeTracker.DayNum.ToString();
        return true;
    }*/
}
