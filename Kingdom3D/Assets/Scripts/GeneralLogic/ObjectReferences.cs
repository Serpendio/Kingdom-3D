using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectReferences : MonoBehaviour
{
    public static ObjectReferences instance;

    public GameObject vagrant, villager, archer, builder, farmer, knight;

    public GameObject coin;
    public GameObject rabbit, deer;
    public GameObject greedling, floater, breeder;

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
    }
}
