using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SubjectBase : CreatureBase
{
    public enum Roles
    {
        Villager,
        Builder,
        Archer,
        Farmer,
        Knight
    }

    const float knockbackForce = 500;

    protected Rigidbody rig;
    protected float stateTime;
    public Transform target;

    public Roles role;
    int coins;
    protected int maxCoins;

    protected virtual void Awake()
    {
        rig = GetComponent<Rigidbody>();

        maxCoins = new int[] { 2, 2, 16, 10, 12 }[(int)role];
    }

    public override void Damage(float damage, Vector3 source)
    {
        rig.AddForce((source - transform.position + Vector3.up).normalized * knockbackForce);

        if (coins > 0)
        {
            var coinsLost = Mathf.Min(coins, Mathf.CeilToInt(damage));
            coins -= coinsLost;
            for (int i = 0; i < coinsLost; i++)
            {
                StaticFunctions.ThrowCoin(transform.position);
            }
        }
        else
        {
            Die();
        }
    }

    protected override void Die()
    {
        if (role != Roles.Villager)
        {
            var obj = Instantiate(ObjectReferences.Instance.villager, transform.position, transform.rotation, transform.parent);
            obj.GetComponent<SubjectBase>().role = Roles.Villager;
            obj.GetComponent<Rigidbody>().velocity = rig.velocity;
        }
        else
        { 
            // perhaps this should just be a different override?
            var obj = Instantiate(ObjectReferences.Instance.vagrant, transform.position, transform.rotation, transform.parent);
            obj.GetComponent<Rigidbody>().velocity = rig.velocity;
        }

        Destroy(gameObject);
    }
}
