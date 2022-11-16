using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassScript : MonoBehaviour
{
    float t;
    [SerializeField, Min(.5f)] float spawnDelay = 4f;
    [SerializeField, Range(0f, 1f)] float spawnChance = 0.3f;
    [SerializeField] GameObject rabbit;

    void Update()
    {
        t += Time.deltaTime;

        if (t >= spawnDelay)
        {
            if (Random.Range(0f, 1f) <= spawnChance)
            {
                Instantiate(rabbit, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
                t = 0;
            }
        }
    }
}
