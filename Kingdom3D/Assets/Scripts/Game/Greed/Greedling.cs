using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greedling : CreatureBase
{
    public bool fromNightlyPortal; // true if spawned from portal during nightly wave

    private void Awake()
    {
        if (fromNightlyPortal)
            GreedTracker.portalGreedlings.Add(transform);
        else
            GreedTracker.bredGreedlings.Add(transform);
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if collision is crown, coin, hermit, dog, etc.
            // pickup item
            // EntityTracker.OnDeath... (ok, it technically didn't die but is treated the same)
            // fromNightlyPortal = true
            // retreat to nearest portal
    }

    public override void Damage(float damage, Vector3 source)
    {
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
        else if (health == 1)
        {
            // lose mask
        }
    }

    protected override void Die()
    {
        if (!fromNightlyPortal)
        {
            GreedTracker.OnDeath(Greed.Greedling, transform);
        }
    }

    public void Retreat() // to be called at dawn or once an item has been collected
    {
        //
    }
}
