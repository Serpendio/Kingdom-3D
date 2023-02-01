using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains each command to be entered into the console.
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
    public static bool map_show(ref string message)
    {
        //toggle the visability
        //if (problem)
            //errorMessage = "message"
            //return false
        return true;
    }

    public static bool time_setscale(ref string message, float timeScale)
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

    public static bool time_getscale(ref string message)
    {
        message = Time.timeScale.ToString("n2");
        return true;
    }
}
