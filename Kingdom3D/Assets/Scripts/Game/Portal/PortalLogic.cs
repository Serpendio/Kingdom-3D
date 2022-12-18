using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalLogic : MonoBehaviour
{
    // could spawn greedlings from all portals each night
    // would need a wave lined up and some spawned at each portal

    [SerializeField] Collider defenceRange;
    [SerializeField] GameObject[] greed;

    private void Awake()
    {
        GameController.Instance.portals.Add(this);

        Debug.LogWarning("WARN: If you are seeing this, greedling speed is still not being used");

        distanceFromOuterWall = transform.position.magnitude; // for now just using distance to camp
        addDelay = Random.Range(2.5f, 5f);

        TimeTracker.OnNoonPassed += () => { defenceRange.enabled = true; };
    }

    float distanceFromOuterWall;
    float timeToSend; // could have these updated whenever a wall is built/destroyed/upgraded/repaired
    public bool sendingWave;
    float spawnDelay;
    float addDelay;
    public List<Greed> greedQueue = new();
    int health;

    float greedlingSpeed = 5; // temporary value, pls replace with the actual variable

    private void Update()
    {
        timeToSend = distanceFromOuterWall / greedlingSpeed;

        if (greedQueue.Count > 0)
        {
            if (spawnDelay <= 0)
            {
                Instantiate(greed[(int)greedQueue[0]]);

                greedQueue.RemoveAt(0);

                if (greedQueue.Count == 0 && sendingWave)
                {
                    defenceRange.enabled = false;
                    nearbySubjects.Clear(); 
                }
                spawnDelay = Random.Range(0.4f, 0.8f);
            }
            else
                spawnDelay -= Time.deltaTime;
        }

        if (!sendingWave && nearbySubjects.Count > 0)
        {
            if (addDelay <= 0)
            {
                greedQueue.Add((Greed)Random.Range(0, 2));
                addDelay = Random.Range(3f, 5f);
            }
            else
                addDelay -= Time.deltaTime;
        }

    }

    readonly List<Transform> nearbySubjects = new();
    private void OnTriggerEnter(Collider other)
    {
        if (StaticFunctions.IsLayerInMask(GameController.Instance.villagerMask, other.gameObject.layer))
        {
            nearbySubjects.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        nearbySubjects.Remove(other.transform);

        if (nearbySubjects.Count == 0)
        {
            addDelay = Random.Range(2.5f, 5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
