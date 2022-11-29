using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubjectBase : CreatureBase
{
    bool employed;
    int coins;

    public override void Damage(float Damage, Vector3 source)
    {
        throw new System.NotImplementedException();
    }

    protected override void Die()
    {
        if (employed)
        {

        }
    }
}
