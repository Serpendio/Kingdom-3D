using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectReferences : MonoBehaviour
{
    public static ObjectReferences Instance;

    public GameObject vagrant, villager, archer, builder, farmer, knight;
    [Tooltip("Order the same as roles")] public List<GameObject> toolsForSubjects;

    public GameObject coin, coinPlaceholder;
    public GameObject rabbit, deer;
    public GameObject greedling, floater, breeder;

    public GameObject rock, tallGrass, tree, mound;
    public GameObject scaffolding;
    public GameObject tower1, tower2, tower3, tower4;
    public GameObject wall1, wall2, wall3, wall4;
    public GameObject destroyedWall1, destroyedWall2, destroyedWall3, destroyedWall4;
    public GameObject gate1, gate2, gate3, gate4;
    public GameObject sideGate1, sideGate2, sideGate3, sideGate4;
    public GameObject keep1, keep2, keep3, keep4;

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
