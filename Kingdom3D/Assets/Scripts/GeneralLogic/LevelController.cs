using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    [Header("For heirarchy readability")]
    public Transform resources;
    public Transform subjects;
    public Transform tools;
    public Transform greed;
    public Transform buildings;
    public Transform wallsNGates;
    public Transform mounds;
    public static Transform player;

    public static List<Farm> farms;
    public static List<PortalLogic> portals = new();
    public static Zone[] zones;
    private static readonly Heap<BuildJob> jobs = new(256); // surely you're not going to have more than 256 jobs going?
    public static List<Polar> outerWallPositions = new();

    public static int numCentralZones;
    [SerializeField, Min(100)] public float islandRadius = 400f;

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

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void UpdatePortalTimes() // called at start, and when a wall is destroyed or created (/ upgraded?)
    {
        
    }

    public float GetDistanceToOuterWall(Vector3 position)
    {
        //perhaps consider outer wall as a bezier or nurbs circle?
        // if inside outer wall return zero
        // if outside the outer wall return distance

        return position.magnitude;
        
        // time to move = time to arrive - (dist from wall / moveSpeed)     // could perhaps round to nearest hour
    }

    public static bool GetJob(out BuildJob job)
    {
        if (jobs.Count > 0)
        {
            job = jobs[0];
            return true;
        }
        else 
        {
            job = null;
            return false;
        }
    }

    public static bool CreateJob(GameObject[] buildings, int numBuilders, int requiredHits, bool ignoreScaffolding)
    {
        new BuildJob(buildings, numBuilders, requiredHits, ignoreScaffolding, out bool successful);
        return successful;
    }

    public static void AddJob(BuildJob job)
    {
        jobs.Add(job);
    }

    public static void RemoveJob(BuildJob job)
    {
        jobs.Remove(job);
    }

    public static bool JobExists(BuildJob job)
    {
        return jobs.Contains(job);
    }
}
