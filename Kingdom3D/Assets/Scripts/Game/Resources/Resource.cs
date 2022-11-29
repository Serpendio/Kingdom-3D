using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public bool isCollectable;
    public ResourceType resourceType;
    bool prioritisePlayer;

    float timeSinceDrop = 0;

    private void Start()
    {

    }

    public void Drop()
    {
        isCollectable = true;
    }

    List<GameObject> overlappingVillagers = new();
    private void OnTriggerEnter(Collider other)
    {
        if (StaticFunctions.IsLayerInMask(GameController.instance.villagerMask, other.gameObject.layer)) // sort this out, if villager, if prioritise player, if knight
            overlappingVillagers.Add(other.gameObject);

    }

    private void OnTriggerExit(Collider other)
    {
        overlappingVillagers.Remove(other.gameObject); 
    }

    private void Update()
    {
        if (isCollectable)
        {
            //knights and greed skip the wait.
        }
    }
}
