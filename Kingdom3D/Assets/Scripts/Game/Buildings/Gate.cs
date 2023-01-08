using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : BuildingBase
{
    public enum Levels
    {
        level1,
        level2,
        level3,
        level4
    }

    public List<GameObject> connectedWalls;
    public Zone[] connectedZones = new Zone[2];
    public bool isDestroyed;
    public Levels level;

    public void UpgradeWalls()
    {
        var wallToSpawn =
            isDestroyed ?
                (level == Levels.level1 ? ObjectReferences.Instance.wall1 :
                level == Levels.level2 ? ObjectReferences.Instance.wall2 :
                level == Levels.level3 ? ObjectReferences.Instance.wall3 :
                ObjectReferences.Instance.wall4)
            :
                (level == Levels.level1 ? ObjectReferences.Instance.wall2 :
                level == Levels.level2 ? ObjectReferences.Instance.wall3 :
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
                (level == Levels.level1 ? ObjectReferences.Instance.gate1 :
                level == Levels.level2 ? ObjectReferences.Instance.gate2 :
                level == Levels.level3 ? ObjectReferences.Instance.gate3 :
                ObjectReferences.Instance.gate4)
            :
                (level == Levels.level1 ? ObjectReferences.Instance.gate2 :
                level == Levels.level2 ? ObjectReferences.Instance.gate3 :
                ObjectReferences.Instance.gate4);

        temp = Instantiate(wallToSpawn, transform.position, transform.rotation, transform.parent);
        temp.GetComponent<Gate>().connectedWalls = connectedWalls;

        // if kingdom is safe, temp.GetComponent<Gate>().OpenGate();

        Destroy(gameObject);
    }

    public void OpenGate()
    {
        // play open anim
        // update astar & collider
    }

    public void CloseGate()
    {
        // play close anim
        // update astar & collider
    }
}
