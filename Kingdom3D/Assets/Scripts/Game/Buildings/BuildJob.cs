using System.Linq;
using UnityEngine;

public interface IBuilding 
{
    // allows subscribing to onBuildIncrement and Complete
    public void PassLinkedJob(BuildJob job); 
}

public class BuildJob : IHeapItem<BuildJob>
{
    private int heapIndex;
    public int HeapIndex { get => heapIndex; set => heapIndex = value; }

    private int hitsGiven = 0;
    private readonly int requiredHits;
    private readonly bool ignoreScaffolding;
    private readonly Builder[] builders;
    private readonly Scaffolding[] scaffolding;
    private readonly IBuilding[] buildings;

    public event System.Action OnBuildIncrement;
    public event System.Action OnBuildComplete;

    public BuildJob(GameObject[] buildings, int numBuilders, int requiredHits, bool ignoreScaffolding, out bool successful)
    {
        IBuilding GetIBuilding(GameObject building)
        {
            IBuilding comp = (IBuilding)building.GetComponent(typeof(IBuilding)); 
            comp.PassLinkedJob(this); 
            return comp;
        }

        this.buildings = buildings.Where(b => b.GetComponent(typeof(IBuilding)) != null).Select(b => GetIBuilding(b)).ToArray();
        if (this.buildings.Length == 0)
        {
            successful = false;
            return;
        }

        successful = true;
        builders = new Builder[numBuilders];

        if (CheatSettings.instantBuild)
        {
            this.ignoreScaffolding = true;
            CompleteUpgrade();
            return;
        }

        LevelController.AddJob(this);

        this.requiredHits = requiredHits;
        this.ignoreScaffolding = true;

        if (!ignoreScaffolding)
        {
            scaffolding = new Scaffolding[buildings.Length];
            for (int i = 0; i < scaffolding.Length; i++)
                scaffolding[i] = Object.Instantiate(ObjectReferences.Instance.scaffolding, ((MonoBehaviour)this.buildings[i]).transform).GetComponent<Scaffolding>();
        }
    }

    public void IncrementUpgrade()
    {
        hitsGiven++;

        OnBuildIncrement.Invoke();

        if (hitsGiven == requiredHits)
        {
            CompleteUpgrade();
            return;
        }

        if (!ignoreScaffolding)
            foreach (Scaffolding scaffold in scaffolding)
                scaffold.UpdateScaffolding(hitsGiven / (float)(requiredHits - 1));
    }

    private void CompleteUpgrade()
    {
        // show graphics, activate collider (perhaps, though it makes sense to be permanently enabled), free linked builders & destroy scaffolding

        OnBuildComplete.Invoke();

        foreach (var builder in builders)
        {
            if (builder != null) builder.targetBuilding = null;
        }

        if (!ignoreScaffolding)
        {
            for (int i = 0; i < scaffolding.Length; i++)
            {
                Object.Destroy(scaffolding[i]);
                ((MonoBehaviour)buildings[i]).transform.GetChild(0).gameObject.SetActive(true); // assumes child 0 is the graphics
            }
        }

        LevelController.RemoveJob(this);
    }

    public void AddBuilder(Builder builder)
    {
        builders[System.Array.IndexOf(builders, null)] = builder;
        if (!System.Array.Exists(builders, null))
        {
            LevelController.RemoveJob(this);
        }
    }

    public void RemoveBuilder(Builder builder)
    {
        if (builders.Contains(builder))
        {
            builders[System.Array.IndexOf(builders, builder)] = null;
            LevelController.AddJob(this);
        }
    }

    public Transform GetRandomBuilding()
    {
        return ((MonoBehaviour)buildings[Random.Range(0, builders.Length)]).transform;
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
