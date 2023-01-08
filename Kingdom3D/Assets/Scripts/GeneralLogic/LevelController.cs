using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    public Transform resources;
    public Transform subjects;
    public Transform tools;
    public Transform greed;
    public Transform buildings;
    public List<Farm> farms;
    public List<PortalLogic> portals = new();
    public List<Zone> zones;

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

    public float GetDistanceToOuterWall(Vector3 position)
    {
        //perhaps consider outer wall as a bezier or nurbs circle?
        // if inside outer wall return zero
        // if outside the outer wall return distance

        return position.magnitude;
        
        // time to move = time to arrive - (dist from wall / moveSpeed)     // could perhaps round to nearest hour
    }
}
