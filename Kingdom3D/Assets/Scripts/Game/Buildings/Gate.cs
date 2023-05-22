using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Gate : MonoBehaviour, IBuilding
{
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

    public struct ConnectionInfo
    {
        public Zone zone1, zone2;
        public int index1, index2;

        /// <param name="zone1">zone this connection upgrades</param>
        /// <param name="index1">index of this gate & zone 2 in zone 1</param>
        /// <param name="zone2">The secondary zone</param>
        /// <param name="index2">index of this gate & zone 1 in zone 2</param>
        public ConnectionInfo(Zone zone1, int index1, Zone zone2, int index2)
        {
            this.zone1 = zone1;
            this.zone2 = zone2;
            this.index1 = index1;
            this.index2 = index2;
        }
    }

    [SerializeField] private Animator anim;
    Collider colliderComp;

    public List<GameObject> linkedWalls;
    public ConnectionInfo connectionInfo;
    public bool isDestroyed, isSideways;
    public WallLevels level;
    [SerializeField, Min(1)] int maxHealth;
    int health;

    public void Start()
    {
        colliderComp = GetComponent<Collider>();
        health = maxHealth;

        if (!builtViaJob) FinishBuild();

        SafetyCheck.OnKingdomSafe += OpenGate;
    }

    /// <summary>
    /// Repairs or upgrades all connectedWalls
    /// </summary>
    public void RebuildWalls(bool shouldRepair)
    {
        GameObject wallToSpawn, gateToSpawn;
        WallLevels spawnLevel = shouldRepair ? level : level + 1;

        if (isSideways)
        {
            wallToSpawn = (spawnLevel == WallLevels.level1 ? ObjectReferences.Instance.sideWall1 :
                            spawnLevel == WallLevels.level2 ? ObjectReferences.Instance.sideWall2 :
                            spawnLevel == WallLevels.level3 ? ObjectReferences.Instance.sideWall3 :
                            ObjectReferences.Instance.sideWall4);
            gateToSpawn = (spawnLevel == WallLevels.level1 ? ObjectReferences.Instance.sideGate1 :
                            spawnLevel == WallLevels.level2 ? ObjectReferences.Instance.sideGate2 :
                            spawnLevel == WallLevels.level3 ? ObjectReferences.Instance.sideGate3 :
                            ObjectReferences.Instance.sideGate4);
        }
        else
        {
            wallToSpawn = (spawnLevel == WallLevels.level1 ? ObjectReferences.Instance.wall1 :
                            spawnLevel == WallLevels.level2 ? ObjectReferences.Instance.wall2 :
                            spawnLevel == WallLevels.level3 ? ObjectReferences.Instance.wall3 :
                            ObjectReferences.Instance.wall4);
            gateToSpawn = (spawnLevel == WallLevels.level1 ? ObjectReferences.Instance.gate1 :
                            spawnLevel == WallLevels.level2 ? ObjectReferences.Instance.gate2 :
                            spawnLevel == WallLevels.level3 ? ObjectReferences.Instance.gate3 :
                            ObjectReferences.Instance.gate4);
        }

        GameObject tempObj;
        for (int i = 0; i < linkedWalls.Count; i++)
        {
            tempObj = Instantiate(wallToSpawn, linkedWalls[i].transform.position, 
                linkedWalls[i].transform.rotation, linkedWalls[i].transform.parent);
            tempObj.GetComponent<Wall>().parent = this;
            
            Destroy(linkedWalls[i]);
            linkedWalls[i] = tempObj;
        }

        // spawn new gate
        tempObj = Instantiate(gateToSpawn, transform.position, transform.rotation, transform.parent);
            
        Gate newGate = tempObj.GetComponent<Gate>();
        newGate.linkedWalls = linkedWalls;
        newGate.connectionInfo = connectionInfo;

        connectionInfo.zone1.neighbourInfos[connectionInfo.index1].gate = newGate;
        connectionInfo.zone2.neighbourInfos[connectionInfo.index2].gate = newGate;

        Destroy(gameObject);
    }

    public void UpgradeZone()
    {
        connectionInfo.zone1.UpgradeWalls();
    }

    public void OpenGate()
    {
        return;

        // play open anim
        //anim.Play("Open");

        // update astar & collider
        //colliderComp.enabled = false;
    }

    public void CloseGate()
    {
        // play close anim
        //anim.Play("Close");

        // update astar & collider
        //colliderComp.enabled = true;
    }

    public void Damage(int damage)
    {
        health -= damage;

        // update damage on animator
        // walls.animator.SetFloat("damage", health / (float)maxHealth);

        if (health <= 0)
        {
            DestroyWalls();
        }
    }

    private void DestroyWalls()
    {
        // disable colliders, update astar
    }

    bool builtViaJob = false;
    public void PassLinkedJob(BuildJob job)
    {
        builtViaJob = true;
        job.OnBuildComplete += FinishBuild;
    }


    GameObject mapObj;
    public void FinishBuild()
    {
        mapObj = Map.Instance.CreateMapObject(Map.ObjectTypes.Gate, (int)level);
        mapObj.transform.localRotation = Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.y + (isSideways ? 90 : 0));
        mapObj.transform.localPosition = new(transform.position.x, transform.position.z);
    }

    private void OnDestroy()
    {
        if (mapObj != null) Destroy(mapObj);
    }
}
