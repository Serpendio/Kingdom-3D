using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBob : MonoBehaviour
{
    public float t;

    [SerializeField] float spinSpeed = 1f;
    [SerializeField] float bobSpeed = 1f;
    [SerializeField] float bobHeight = 0.2f;

    void Update()
    {
        t += Time.deltaTime;
        transform.SetPositionAndRotation(
            bobHeight * Mathf.Sin(t * bobSpeed) * transform.up,
            Quaternion.Euler(spinSpeed * t * transform.up));
    }

    public void MatchSpin(ResourceBob bob)
    {
        t = bob.t;
    }
}
