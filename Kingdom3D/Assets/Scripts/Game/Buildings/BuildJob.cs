using Den.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IBuilding { }

public class BuildJob : MonoBehaviour, IHeapItem<BuildJob>
{
    private int heapIndex;
    public int HeapIndex { get => heapIndex; set => heapIndex = value; }

    private int hitsGiven = 0;
    private int requiredHits;
    private int numBuilders;
    private bool ignoreScaffolding;
    private Builder[] builders;
    private Scaffolding[] scaffolding;
    private GameObject[] buildings;

    public event System.Action OnBuildIncrement;

    public void Setup(int requiredHits, int numBuilders, GameObject[] buildings, bool ignoreScaffolding)
    {
        this.requiredHits = requiredHits;
        this.numBuilders = numBuilders;
        this.buildings = buildings;
        this.ignoreScaffolding = true;
    }

    private void StartUpgrade()
    {
        if (!ignoreScaffolding)
        {
            scaffolding = new Scaffolding[buildings.Length];
            for (int i = 0; i < scaffolding.Length; i++)
                scaffolding[i] = Instantiate(ObjectReferences.Instance.scaffolding, buildings[i].transform).GetComponent<Scaffolding>();
        }
    }

    public void IncrementUpgrade()
    {
        hitsGiven++;

        OnBuildIncrement.Invoke();

        if (hitsGiven == requiredHits)
        {
            FinishUpgrade();
            return;
        }

        if (!ignoreScaffolding)
            foreach (Scaffolding scaffold in scaffolding)
                scaffold.UpdateScaffolding(hitsGiven / (float)(requiredHits - 1));
    }

    private void FinishUpgrade()
    {
        // show graphics, activate collider (perhaps, though it makes sense to be permanently enabled), free linked builders & destroy scaffolding

        foreach (var builder in builders)
        {
            if (builder != null) builder.targetBuilding = null;
        }

        if (!ignoreScaffolding)
        {
            for (int i = 0; i < scaffolding.Length; i++)
            {
                Destroy(scaffolding[i]);
                buildings[i].transform.GetChild(0).gameObject.SetActive(true); // assumes child 0 is the graphics
            }
            scaffolding = null;
        }

        LevelController.Jobs.Remove(this);

        Destroy(gameObject);
    }

    private void Start()
    {
        builders = new Builder[numBuilders];
        LevelController.Jobs.Add(this);
        StartUpgrade();
    }

    public void AddBuilder(Builder builder)
    {
        builders[System.Array.IndexOf(builders, null)] = builder;
        if (!System.Array.Exists(builders, null))
        {
            LevelController.Jobs.RemoveFirst();
        }
    }

    public void RemoveBuilder(Builder builder)
    {
        if (builders.Contains(builder))
        {
            builders[builders.Find(builder)] = null;
            if (!LevelController.Jobs.Contains(this))
            {
                LevelController.Jobs.Add(this);
            }
        }
    }

    public Transform GetRandomBuilding()
    {
        return buildings[Random.Range(0, builders.Length)].transform;
    }

    public int CompareTo(BuildJob other)
    {
        return GetRandomBuilding().position.sqrMagnitude
            .CompareTo(other.GetRandomBuilding().position.sqrMagnitude);
    }

    /*static bool GetNearestAvailableBuilder(Vector3 position, ref Builder builder)
    {
        builder = LevelController.Instance.subjects
            .GetComponentsInChildren<SubjectBase>() // get every subjectBase
            .OfType<Builder>() // remove any that aren't builders
            .Where(b => b.targetBuilding == null) // remove any that are already at work
            .OrderBy(b => (b.transform.position - position).sqrMagnitude) // 
            .FirstOrDefault();

        return builder != null;
    }*/
}
