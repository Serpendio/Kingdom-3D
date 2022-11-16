using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpeedSlider : MonoBehaviour
{
    public void SetSpeed(float speed)
    {
        Time.timeScale = speed;
    }
}
