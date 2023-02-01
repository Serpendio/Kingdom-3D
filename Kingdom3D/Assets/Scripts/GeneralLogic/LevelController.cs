using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Header("For heirarchy readability")]
    public Transform resources;
    public Transform subjects;
    public Transform tools;
    public Transform greed;
    public Transform buildings;
    public Transform wallsNGates;
    public Transform mounds;
    public Transform player;

    public static List<Farm> farms;
    public static List<PortalLogic> portals = new();
    public static Zone[] zones;
    public static Heap<BuildJob> Jobs { get; private set; } = new(256); // surely you're not going to have more than 256 jobs going?
    public static List<Polar> outerWallPositions = new();

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

        if (player == null)
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
        if (Jobs.Count > 0)
        {
            job = Jobs[0];
            return true;
        }
        else 
        {
            job = null;
            return false;
        }
    }

    public static void AddJob(BuildJob job)
    {
        Jobs.Add(job);
    }
}
