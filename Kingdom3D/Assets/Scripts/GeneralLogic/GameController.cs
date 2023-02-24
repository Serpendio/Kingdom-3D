using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Gold,
    Gem
}

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public LayerMask obstacleMask, wildlifeMask, enemyMask, villagerMask;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        new SafetyCheck();
        new GreedTracker();
    }
}
