using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Gold,
    Gem
}

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public LayerMask obstacleMask, wildlifeMask, enemyMask, villagerMask;
    public Transform player;
    public GameObject coinObj;

    public List<Transform> coins = new List<Transform>();
    public List<PortalLogic> portals = new List<PortalLogic>();

    public int gold;
    public int gems;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        new SafetyCheck();
    }
}
