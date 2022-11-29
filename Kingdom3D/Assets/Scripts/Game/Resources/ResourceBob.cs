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
        transform.SetLocalPositionAndRotation(
            bobHeight * (Mathf.Sin(t * bobSpeed)+1f) * transform.up,
            Quaternion.AngleAxis(spinSpeed * t, transform.parent.up));
    }

    public void MatchSpin(ResourceBob bob)
    {
        t = bob.t;
    }
}
