using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : BuildingBase
{
    public enum Levels
    {
        level1,
        level2,
        level3,
        level4
    }
    
    [HideInInspector] public List<Wall> connections;
    public bool isGate;
    public bool isDestroyed;
    public Levels level;

    bool allFixed;

    public void UpgradeWalls(Wall connection)
    {
        foreach(Wall wall in connections)
        {
            if (wall == connection || wall.isGate)
                continue;
        }
    }

    int GetNumUpgradeable(Wall connection)
    {
        int num = 1;

        foreach (Wall wall in connections)
        {
            if (wall == connection || wall.isGate)
                continue;
            else
                num += wall.GetNumUpgradeable(this);
        }

        return num;
    }

    int GetNumDestroyed(Wall connection)
    {
        int num = 0;
        if (isDestroyed)
            num++;

        foreach (Wall wall in connections)
        {
            if (wall == connection || wall.isGate)
                continue;
            else
                num += wall.GetNumDestroyed(this);
        }

        return num;
    }

    public void FixWalls()
    {

    }

    public void AcceptPayment()
    {
        if (false) ;
    }
    
    public void CheckInteractiveVisablity()
    {
        Interactable interactable = GetComponentInChildren<Interactable>();

        int temp = GetNumDestroyed(null);
        allFixed = temp == 0;

        if (!allFixed)
        {
            interactable.cost = Mathf.CeilToInt(temp * 0.2f * ((int)level + 1));
        }
        else
        {
            temp = GetNumUpgradeable(null);
            interactable.cost = Mathf.CeilToInt(temp * 0.4f * ((int)level + 1));
        }
    }
}
