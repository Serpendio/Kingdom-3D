using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    bool isWithering;
    float timeFarmed;

    public Farmer linkedFarmer;
    bool isFarming;
    public bool isValid = true;
    float timeSinceFarmed;
    const float cropHeight = 2f;
    const float farmTime = TimeTracker.noon - TimeTracker.dawn; // 8 hours

    BoxCollider farmBounds;
    GameObject cropObj;

    private void Awake()
    {
        farmBounds = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (isFarming)
        {
            timeFarmed += Time.deltaTime;
            if (timeFarmed >= farmTime)
            {
                timeFarmed = 0;
                for (int i = 0; i < 5; i++)
                    StaticFunctions.ThrowCoin(transform.position + new Vector3(Random.Range(-Farm.fieldSize / 3f, Farm.fieldSize / 3f), 0.5f, Random.Range(-Farm.fieldSize / 3f, Farm.fieldSize / 3f)));
            }
            cropObj.transform.position = transform.position - (1 - timeFarmed / farmTime) * cropHeight * Vector3.up;
        }
        else if (isWithering)
        {
            timeFarmed -= Time.deltaTime * 8;
            if (timeFarmed <= 0)
            {
                isWithering = false;
                timeFarmed = 0;
                Destroy(cropObj);
                if (!isValid)
                    Destroy(gameObject);
            }
            else
                cropObj.transform.position = transform.position - (1 - timeFarmed / farmTime) * cropHeight * Vector3.up;
        }
        else if (timeFarmed != 0)
        {
            timeSinceFarmed += Time.deltaTime;
            if (timeSinceFarmed > TimeTracker.midnight * 1.5f)
            {
                isWithering = true;
                Destroy(cropObj);
                cropObj = Instantiate(ObjectReferences.Instance.cropWithered, transform.position - Vector3.up * cropHeight, transform.rotation, transform);
            }
        }
    }

    public void StartFarming()
    {
        isFarming = true;
        timeSinceFarmed = 0f;
        if (timeFarmed == 0)
            cropObj = Instantiate(ObjectReferences.Instance.crop, transform.position - Vector3.up * cropHeight, transform.rotation, transform);
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
            isWithering = true;
            if (linkedFarmer != null) linkedFarmer.CancelFarm();
        }
    }

}
