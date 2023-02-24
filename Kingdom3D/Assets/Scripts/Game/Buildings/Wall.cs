using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IBuilding
{
    public Gate parent;

    private void Start()
    {
        if (!builtViaJob) FinishBuild();

        // update astar
    }

    public void TakeDamage()
    {

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
        mapObj = Map.Instance.CreateMapObject(Map.ObjectTypes.Wall, (int)parent.level);
        mapObj.transform.localRotation = Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.y + (parent.isSideways ? 90 : 0));
        mapObj.transform.localPosition = new(transform.position.x, transform.position.z);
    }

    private void OnDestroy()
    {
        if (mapObj != null) Destroy(mapObj);
    }
}
