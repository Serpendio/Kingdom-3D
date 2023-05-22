using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Farmer : SubjectBase
{
    Field linkedField;
    Farm linkedFarm;
    bool isfarming;
    float checkTime = 0f;
    Vector3 target;

    protected override void Awake()
    {
        base.Awake();

        SafetyCheck.OnKingdomSafe += StartFarming;
        TimeTracker.AddActionAtTime(TimeTracker.sunset, EndFarming);
    }

    private void Update()
    {
        checkTime += Time.deltaTime;
        if (checkTime >= 3f)
        {
            checkTime = 0f;
            if (linkedFarm == null)
            {
                FindFarm();
            }
        }
    }

    public void StartFarming()
    {
        //if field is still valid
            // move to farm center
        //else
            //look for new field

        if (linkedField != null && linkedField.isValid)
        {
            
        }
        else
        {
            FindFarm();
        }
    }

    private void EndFarming()
    {
        // move to town center or sheltered farm
        if (linkedFarm == null || linkedFarm.level == 0)
        {
            target = LevelController.zones[Random.Range(0, LevelController.numCentralZones)].GetRandomEmptyPoint();
        }
        else
        {
            target = linkedFarm.transform.position;
        }
    }

    public void CancelFarm()
    {
        linkedFarm = null;
        EndFarming();
    }

    void FindFarm()
    {
        foreach (Farm farm in LevelController.farms)
        {
            if (farm.FindEmptyField(ref linkedField))
            {
                break;
            }
        }
    }
}
