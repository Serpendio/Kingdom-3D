using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading;

public class TimeUI : MonoBehaviour
{
    TextMeshProUGUI text;
    //float temp;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        //temp = Time.realtimeSinceStartup;

        //TimeTracker.OnDawnPassed += () => print((Time.realtimeSinceStartup - temp).ToString("f6"));
        /*TimeTracker.OnDawnPassed += () => print("Dawn Passed");
        TimeTracker.OnSunrisePassed += () => print("Sunrise Passed");
        TimeTracker.OnNoonPassed += () => print("Noon Passed");
        TimeTracker.OnSunsetPassed += () => print("Sunset Passed");
        TimeTracker.OnMidnightPassed += () => print("Midnight Passed");*/
    }

    void Update()
    {
        int hour = (int)(TimeTracker.CurrentTime / 60);
        int minute = (int)(TimeTracker.CurrentTime % 60);

        text.text = hour.ToString("D2") + ":" + minute.ToString("D2");
    }
}
