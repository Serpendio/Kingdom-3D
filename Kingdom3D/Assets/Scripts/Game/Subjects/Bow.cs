using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField] GameObject arrow;

    public void Fire(float fireSpeed)
    {
        var projectile = Instantiate(arrow, transform.position, transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = transform.forward * fireSpeed;
        projectile.GetComponent<Arrow>().source = transform;
    }
    
}
