using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Villager : SubjectBase
{
    float reobserveTime;
    bool isMoving;

    protected override void Awake()
    {
        base.Awake();
        reobserveTime = Random.Range(2f, 4f);
    }

    private void Update()
    {
        
        if (isMoving)
        {
            rig.velocity = /*StaticFunctions.BirdsEyeDisplacement(transform, target).normalized*/ transform.forward * moveSpeed;
        }

        reobserveTime -= Time.deltaTime;

        if (reobserveTime <= 0)
        {
            target = LevelController.Instance.tools.Cast<Transform>() // gets all children as enumerable as referenced https://answers.unity.com/questions/1282940/get-all-child-transforms-in-target.html
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude) // order by dist sqrd which is faster due to no square root
                .FirstOrDefault();
            
            if (target == null)
            {
                transform.DORotate((Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * transform.rotation).eulerAngles, .5f);
                isMoving = Random.Range(0, 3) == 0; // 33% chance every ~3s to move for ~3s
            }
            else
            {
                float angle = (Quaternion.FromToRotation(transform.forward, target.position - transform.position) * transform.rotation).eulerAngles.y;
                transform.DORotate(angle * Vector3.up, .3f);
                isMoving = true;
            }

            reobserveTime = 3f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
}
