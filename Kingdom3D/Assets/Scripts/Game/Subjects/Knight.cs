using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : CreatureBase
{
    int coins;
    const int maxCoins = 12;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Resource") && other.TryGetComponent(out Resource resource) && resource.resourceType == ResourceType.Gold && resource.isCollectable)
        {
            resource.isCollectable = false;
            resource.transform.DOMove(transform.position, 0.5f).OnComplete(() => { Destroy(resource.gameObject); });
            // play coin collect anim
            coins++;
        }
    }

    public override void Damage(float damage, Vector3 source)
    {
        throw new System.NotImplementedException();
    }

    protected override void Die()
    {
        throw new System.NotImplementedException();
    }
}
