using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RabbitScript : MonoBehaviour
{
    [SerializeField, Min(0.1f)] float moveSpeed;
    [SerializeField, Min(0.1f)] float minDist;
    [SerializeField, Min(0.1f)] float maxDist;
    [SerializeField, Min(0.1f)] float jumpHeight;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject coin;
    bool isMoving;
    Rigidbody rig;

    void Start()
    {
        if (maxDist < minDist)
            maxDist = minDist;
        rig = GetComponent<Rigidbody>();
        StartCoroutine(DoSomething());
    }

    private void Update()
    {
        if (isMoving)
        {
            Vector3 vel = rig.velocity;
            vel.x = transform.forward.x * moveSpeed;
            vel.z = transform.forward.z * moveSpeed;
            if (Physics.CheckSphere(groundCheck.position, 0.01f, groundLayer))
            {
                vel.y = Mathf.Sqrt(2 * 9.81f * jumpHeight);
            }
            rig.velocity = vel;
        }

    }

    IEnumerator DoSomething()
    {
        float t;
        while (true)
        {
            if (Random.Range(0, 4) == 0)
            {
                t = Random.Range(minDist, maxDist) / moveSpeed;
                //transform.Rotate(Vector3.up, Random.Range(0f, 360f));
                //transform.DORotateQuaternion(Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * transform.rotation, .5f);
                transform.DORotate((Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * transform.rotation).eulerAngles, .5f);
                isMoving = true;
            }
            else
            {
                isMoving = false;
                t = Random.Range(2f, 4f);
            }
            yield return new WaitForSeconds(t);
        }
    }

    public void Damage()
    {
        isMoving = false;
        StopCoroutine(DoSomething());
        Destroy(rig);
        Destroy(GetComponent<Collider>());
        transform.DOScaleY(0, 0.3f).OnComplete(Kill);
    }

    void Kill()
    {
        Instantiate(coin, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
        Destroy(this);
    }
}
