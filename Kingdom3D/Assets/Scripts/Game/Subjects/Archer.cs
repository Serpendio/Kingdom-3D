using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Archer : MonoBehaviour
{

    [SerializeField, Min(0.1f)] float moveSpeed;
    [SerializeField, Min(0.1f)] float minDist;
    [SerializeField, Min(0.1f)] float maxDist;
    [SerializeField, Min(0.1f)] float spotRange;
    [SerializeField, Min(0.1f)] float attackRange; 
    // could make sure the attack range is < the maximum range from fire speed (errors would happen if archer fired at something it couldn't hit).
    //halving the fire speed (supposedly) quaters the max range
    
    [SerializeField] GameObject coin;
    [SerializeField, Range(10, 60)] float maxFireSpeed = 30;

    bool isMoving;
    Rigidbody rig;
    Transform bow;
    Bow bowScript;

    void Awake()
    {
        if (maxDist < minDist)
            maxDist = minDist;
        rig = GetComponent<Rigidbody>();

        bow = transform.GetChild(0);
        bowScript = bow.GetComponent<Bow>();
    }

    private void Start()
    {
        StartCoroutine(DoSomething());
    }

    private void Update()
    {
        if (isMoving)
        {
            Vector3 vel = rig.velocity;
            vel.x = transform.forward.x * moveSpeed;
            vel.z = transform.forward.z * moveSpeed;
            rig.velocity = vel;
        }
    }

    IEnumerator DoSomething()
    {
        float t;
        while (true)
        {
            Collider nearbyTarget = Physics.OverlapSphere(transform.position, spotRange, GameController.instance.enemyMask) // all greed in range
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude) // order by dist sqrd which is faster due to no square root
                .FirstOrDefault();
            
            if (nearbyTarget == null)
                nearbyTarget = Physics.OverlapSphere(transform.position, spotRange, GameController.instance.wildlifeMask)
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (nearbyTarget != null)
            {
                //float horizontalDist = Vector3.Distance(nearbyCreature.transform.position, bow.position);
                var pos1 = nearbyTarget.transform.position;
                var pos2 = bow.position;
                float horizontalDist = new Vector2(pos1.x - pos2.x, pos1.z - pos2.z).magnitude;
                float verticalDist = pos1.y - pos2.y;

                Physics.Raycast(pos2, pos1 - pos2, out RaycastHit hitInfo, Mathf.Infinity, GameController.instance.obstacleMask | GameController.instance.wildlifeMask);
                bool directShot = hitInfo.collider.transform == nearbyTarget.transform;

                if (horizontalDist <= attackRange)
                {
                    float angle = (Quaternion.FromToRotation(transform.forward, nearbyTarget.transform.position - transform.position) * transform.rotation).eulerAngles.y;
                    transform.DORotate(angle * Vector3.up, .3f);
                    yield return new WaitForSeconds(.5f);

                    // as said above, halving the fire speed (supposedly) quaters the max range
                    float firespeed = maxFireSpeed; //directShot ? maxFireSpeed : maxFireSpeed / 1.5f;
                    float sqrFireSpeed = firespeed * firespeed;

                    // simplified equation for the angle from suvat when initial velocity, acceleration and distance is known: + means direct shot; - means overhead shot;
                    if (directShot)
                        angle = Mathf.Rad2Deg * Mathf.Atan((-sqrFireSpeed
                                + Mathf.Sqrt(sqrFireSpeed * sqrFireSpeed
                                - Physics.gravity.y * (2 * sqrFireSpeed * verticalDist
                                + horizontalDist * horizontalDist * Physics.gravity.y)))
                                / (Physics.gravity.y * horizontalDist));
                    else
                        angle = Mathf.Rad2Deg * Mathf.Atan((-sqrFireSpeed
                                - Mathf.Sqrt(sqrFireSpeed * sqrFireSpeed
                                - Physics.gravity.y * (2 * sqrFireSpeed * verticalDist
                                + horizontalDist * horizontalDist * Physics.gravity.y)))
                                / (Physics.gravity.y * horizontalDist));

                    bow.localRotation = Quaternion.Euler(-angle, 0, 0); // negative as anticlockwise
                    bowScript.Fire(firespeed);

                    // used speed as main varying factor with fixed horizontal angle (unrealistic and would look terrible)
                    /*var projectile = Instantiate(arrow, firepoint.position, transform.rotation);
                    projectile.GetComponent<Rigidbody>().velocity = 2 * 9.81f * dist / Mathf.Sqrt(2 * 9.81f * (firepoint.position.y - nearbyCreature.transform.position.y)) * transform.forward;
                    print(projectile.GetComponent<Rigidbody>().velocity.magnitude);
                    projectile.GetComponent<Arrow>().source = transform;*/

                    t = Random.Range(4f, 6f);
                }
                else
                {
                    t = horizontalDist - attackRange / moveSpeed;
                    
                    transform.DORotate(Vector3.up * Vector3.Angle(transform.position, nearbyTarget.transform.position), .5f);
                    isMoving = true;
                }
            }
            else
            {
                if (Random.Range(0, 4) == 0)
                {
                    t = Random.Range(minDist, maxDist) / moveSpeed;
                    transform.DORotate((Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * transform.rotation).eulerAngles, .5f);
                    isMoving = true;
                }
                else
                {
                    isMoving = false;
                    t = Random.Range(2f, 4f);
                }
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
