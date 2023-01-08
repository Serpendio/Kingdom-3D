using Den.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    [HideInInspector] public bool isUpgrading;
    protected int hitsGiven;
    protected int requiredHits;
    protected int numBuilders;
    protected bool ignoreScaffolding;
    private Builder[] builders;
    private Scaffolding scaffolding;

    private float checkTime;

    public virtual void StartUpgrade()
    {
        isUpgrading = true;

        //if (!ignoreScaffolding)
        //    scaffolding = Instantiate(ObjectReferences.Instance.scaffolding, transform).GetComponent<Scaffolding>();
    }

    public virtual void IncrementUpgrade()
    {
        hitsGiven++;


        if (hitsGiven == requiredHits)
        {
            FinishUpgrade();
            return;
        }

        // perhaps increment scaffolding

        if (!ignoreScaffolding)
            scaffolding.UpdateScaffolding(hitsGiven / (float)(requiredHits - 1));
    }

    public virtual void FinishUpgrade()
    {
        // show graphics, activate collider (perhaps, though it makes sense to be permanently enabled), free linked builders & destroy scaffolding

        isUpgrading = false;

        foreach (var builder in builders)
        {
            if (builder != null) builder.targetBuilding = null;
        }

        if (!ignoreScaffolding)
        {
            Destroy(scaffolding);
            transform.GetChild(0).gameObject.SetActive(true); // assumes child 0 is the graphics
        }
    }

    protected virtual void Start()
    {
        builders = new Builder[numBuilders];
        checkTime = 3f;
        StartUpgrade();
    }

    protected virtual void Update()
    {
        // if is upgrading (could perhaps put this in child before calling base.Update
            // if builders contains null
                // try to call new builder to upgrade


        if (isUpgrading)
        {
            if (checkTime <= 0)
            {

                for (int i = 0; i < numBuilders; i++)
                {
                    if (builders[i] == null)
                    {
                        if (GetNearestAvailableBuilder(transform.position, ref builders[i]))
                            builders[i].targetBuilding = this;
                        else
                            break;
                    }
                }
                checkTime = 3f;
            }
            else
            {
                checkTime -= Time.deltaTime;
            }
        }
    }



    static bool GetNearestAvailableBuilder(Vector3 position, ref Builder builder)
    {
        builder = LevelController.Instance.subjects
            .GetComponentsInChildren<SubjectBase>() // get every subjectBase
            .OfType<Builder>() // remove any that aren't builders
            .Where(b => b.targetBuilding == null) // remove any that are already at work
            .OrderBy(b => (b.transform.position - position).sqrMagnitude) // 
            .FirstOrDefault();

        return builder != null;
    }
}
