using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] LayerMask mainMask;
    public Transform source;

    // if it still moves too fast, perhaps use raycasting from last pos to current pos rather than a collider

    private void OnTriggerEnter(Collider other)
    {
        if (mainMask == (mainMask | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out RabbitScript rabbit))
            {
                rabbit.Damage();
            }
            else if (other.TryGetComponent(out Deer deer))
            {
                deer.Damage(1, source.position);
            }
            Destroy(gameObject);
        }
        else if (obstacleMask == (obstacleMask | (1 << other.gameObject.layer)))
        {
            transform.parent = other.transform;
            StartCoroutine(Despawn());
        }
    }

    IEnumerator Despawn()
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
        yield return new WaitForSeconds(20);
        Destroy(gameObject);
    }
}
