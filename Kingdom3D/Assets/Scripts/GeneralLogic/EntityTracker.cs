using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Greed
{
    Greedling,
    MaskedGreedling, // only used for wave linup
    Thief,
    Floater,
    Breeder,
    Combo // for thieves riding atop breeders, only useful for the wave lineup
        // could have a check in breeder destroyed for a parent with name combo to destroy also
}

public class EntityTracker : MonoBehaviour
{
    // defense waves should not be tracked on here as that'll lead to the bell tolling when it shouldn't
    public static List<Transform> portalGreedlings = new(); // those spawned from portals, not during retaliation waves 
    public static List<Transform> bredGreedlings = new(); // from breeders / portals during retaliation waves
    public static List<Transform> crownStealers = new();
    public static List<Transform> floaters = new();
    public static List<Transform> breeders = new();

    public void OnDeath(Greed greedType, Transform instance)
    {
        switch (greedType)
        {
            case Greed.Greedling:
                if (!portalGreedlings.Remove(instance))
                    bredGreedlings.Remove(instance);
                break;
            case Greed.Thief:
                crownStealers.Remove(instance);
                break;
            case Greed.Floater:
                floaters.Remove(instance);
                break;
            case Greed.Breeder:
                breeders.Remove(instance);
                break;
            default:
                break;
        }

        if (bredGreedlings.Count + crownStealers.Count + floaters.Count + breeders.Count == 0)
        {
            if (TimeTracker.CurrentTime > TimeTracker.dawn)
        }
    }
}
