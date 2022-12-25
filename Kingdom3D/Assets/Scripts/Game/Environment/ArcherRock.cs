using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherRock : MonoBehaviour
{
    public void BuildTower()
    {
        Instantiate(ObjectReferences.Instance.tower1,  transform.position, transform.rotation, transform.parent);
        Destroy(gameObject);
    }
}
