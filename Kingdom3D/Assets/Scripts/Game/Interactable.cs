using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onPaidFor;
    public ResourceType resourceType;
    [Min(1)] public int cost = 1;
    [SerializeField] GameObject coin, gem, coinPlaceholder, gemPlaceHolder;

    private void Awake()
    {
        slots = new GameObject[cost];
        filledSlots = new GameObject[cost];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerController>().UpdateInteractables(true, this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().UpdateInteractables(true, this);
            SetCostVisibility(false);
        }
    }

    GameObject[] slots;
    readonly float spread = 1;
    public void SetCostVisibility(bool visible)
    {
        if (visible)
        {
            for (int i = 0; i < cost; i++)
            {
                slots[i] = Instantiate(coinPlaceholder, StaticFunctions.Slerp(transform.GetChild(0).position - (cost-1) / 2f * spread * transform.right, transform.GetChild(0).position + (cost - 1) * spread * transform.right / 2f, i / (cost-1f), transform.GetChild(0).position - transform.up*1), Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
            }
        }
        else
        {
            for (int i = 0; i < cost; i++)
            {
                Destroy(slots[i]);

                if (i < slotsFilled)
                {
                    filledSlots[i].GetComponent<Resource>().Drop();
                }
            }
        }
    }

    GameObject[] filledSlots;
    int slotsFilled = 0;
}
