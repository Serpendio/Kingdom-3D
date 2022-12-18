using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    enum State
    {
        Empty,
        Withering,
        Stage1,
        Stage2,
        Stage3
    }

    float completion = 0f;

    public Farmer linkedFarmer;
    bool isFarming;
    public bool isValid = true;
    float timeSinceFarmed;

    BoxCollider farmBounds;

    private void Awake()
    {
        farmBounds = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (isFarming)
        {
            
        }
        else
        {
            timeSinceFarmed += Time.deltaTime;
            // if 
        }
    }

    public void StartFarming()
    {
        isFarming = true;
        timeSinceFarmed = 0f;
    }

    public void StopFarming()
    {
        isFarming = false;
    }

    public void CheckValidity()
    {
        if (isValid && Physics.CheckBox(farmBounds.center, farmBounds.size/2f, Quaternion.identity, LayerMask.NameToLayer("Obstacles")))
        {
            isValid = false;
            linkedFarmer.StartFarming();
        }
    }

}
