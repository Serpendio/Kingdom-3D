using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectReferences : MonoBehaviour
{
    public static ObjectReferences Instance;

    public GameObject vagrant, villager, archer, builder, farmer, knight;

    public GameObject coin;
    public GameObject rabbit, deer;
    public GameObject greedling, floater, breeder;
    [Tooltip("Order the same as roles")] public List<GameObject> toolsForSubjects;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
