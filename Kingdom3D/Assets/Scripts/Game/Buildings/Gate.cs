using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallLevels
{
    level1,
    level2,
    level3,
    level4
}

public enum WallUpgradeHits
{
    level1 = 8,
    level2 = 20,
    level3 = 64,
    level4 = 80,
}

public class Gate : MonoBehaviour, IBuilding
{
    [SerializeField] private Animator anim;

    public List<GameObject> connectedWalls;
    public Zone[] connectedZones = new Zone[2]; // if it's a ring gate, zone[0] will be the zone this gate upgrades
    public bool isDestroyed;
    public WallLevels level;
    [SerializeField] int health;

    public void Start()
    {
        SafetyCheck.OnKingdomSafe += OpenGate;
    }

    /// <summary>
    /// Repairs or upgrades all connectedWalls depending on if the gate is Destroyed
    /// </summary>
    public void UpgradeWalls() // also repairs
    {
        var wallToSpawn =
            isDestroyed ?
                (level == WallLevels.level1 ? ObjectReferences.Instance.wall1 :
                level == WallLevels.level2 ? ObjectReferences.Instance.wall2 :
                level == WallLevels.level3 ? ObjectReferences.Instance.wall3 :
                ObjectReferences.Instance.wall4)
            :
                (level == WallLevels.level1 ? ObjectReferences.Instance.wall2 :
                level == WallLevels.level2 ? ObjectReferences.Instance.wall3 :
                ObjectReferences.Instance.wall4);

        GameObject temp;
        for (int i = 0; i < connectedWalls.Count; i++)
        {
            temp = Instantiate(wallToSpawn, connectedWalls[i].transform.position, 
                connectedWalls[i].transform.rotation, connectedWalls[i].transform.parent);
            temp.GetComponent<Wall>().parent = this;
            
            Destroy(connectedWalls[i]);
            connectedWalls[i] = temp;
        }

        wallToSpawn =
            isDestroyed ?
                (level == WallLevels.level1 ? ObjectReferences.Instance.gate1 :
                level == WallLevels.level2 ? ObjectReferences.Instance.gate2 :
                level == WallLevels.level3 ? ObjectReferences.Instance.gate3 :
                ObjectReferences.Instance.gate4)
            :
                (level == WallLevels.level1 ? ObjectReferences.Instance.gate2 :
                level == WallLevels.level2 ? ObjectReferences.Instance.gate3 :
                ObjectReferences.Instance.gate4);

        temp = Instantiate(wallToSpawn, transform.position, transform.rotation, transform.parent);
        temp.GetComponent<Gate>().connectedWalls = connectedWalls;

        // if kingdom is safe, temp.GetComponent<Gate>().OpenGate();
        if (SafetyCheck.isKingdomSafe)
            temp.GetComponent<Gate>().OpenGate();

        Destroy(gameObject);
    }

    public void UpgradeZone()
    {
        connectedZones[0].UpgradeWalls(level + 1);
    }

    public void OpenGate()
    {
        // play open anim
        anim.Play("Open");

        // update astar & collider
        GetComponent<Collider>().enabled = false;
    }

    public void CloseGate()
    {
        // play close anim
        anim.Play("Close");

        // update astar & collider
        GetComponent<Collider>().enabled = true;
    }

    public void Damage()
    {

    }

    public void SyncHealth()
    {

    }
}
