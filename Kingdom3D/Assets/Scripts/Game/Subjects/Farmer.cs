using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Farmer : SubjectBase
{
    Field linkedField;
    Farm linkedFarm;

    protected override void Awake()
    {
        base.Awake();

        SafetyCheck.OnKingdomSafe += StartFarming;
        TimeTracker.OnSunsetPassed += EndFarming;
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
    }

    void FindFarm()
    {
        foreach (Farm farm in LevelController.Instance.farms)
        {
            if (farm.FindEmptyField(ref linkedField))
            {
                break;
            }
        }
    }
}
