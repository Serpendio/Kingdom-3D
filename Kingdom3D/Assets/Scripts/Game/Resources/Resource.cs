using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public bool isCollectable;
    public ResourceType resourceType;
    bool prioritisePlayer;

    float timeSinceDrop = 0;

    public Transform target;
    bool reachedTarget;
    Vector3 startPos;
    Quaternion startRotation;
    public const float moveTime = .5f;

    private void Start()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
    }

    public void Drop()
    {
        isCollectable = true;
        GetComponent<Rigidbody>().useGravity = true;
    }

    List<GameObject> overlappingVillagers = new();
    private void OnTriggerEnter(Collider other)
    {
        //if (isCollectable && other.transform == LevelController.player) Destroy(gameObject);

        if (GameController.Instance.villagerMask.Contains(other.gameObject.layer)) // sort this out, if villager, if prioritise player, if knight
            overlappingVillagers.Add(other.gameObject);

    }

    private void OnTriggerExit(Collider other)
    {
        overlappingVillagers.Remove(other.gameObject); 
    }

    private void Update()
    {
        if (isCollectable)
        {
            //knights and greed skip the wait.
        }
        else if (!reachedTarget && target != null)
        {
            transform.position = Vector3.Lerp(startPos, target.position, timeSinceDrop / moveTime);
            transform.rotation = Quaternion.Lerp(startRotation, target.rotation, timeSinceDrop / moveTime);

            if (timeSinceDrop >= moveTime)
            {
                reachedTarget = true;
            }
            timeSinceDrop += Time.deltaTime;
        }
    }

    void SetTarget()
    {

    }

    void FreeTarget()
    {

    }
}
