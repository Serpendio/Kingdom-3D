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

    private void Update()
    {
        transform.LookAt(GameController.Instance.player.transform.position);
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
            other.GetComponent<PlayerController>().UpdateInteractables(false, this);
            SetCostVisibility(false);
        }
    }

    GameObject[] slots;
    readonly float spread = .64f;
    public void SetCostVisibility(bool visible)
    {
        if (visible)
        {
            int y = 0;
            int x;
            int xMax = cost;
            for (int i = 0; i < cost; i++)
            {
                if (cost > 4)
                {
                    x = i % ((cost + 1) / 2);
                    y = i / ((cost + 1) / 2);
                    xMax = cost % 2 == 0 ? cost / 2 : ((cost + 1) / 2 - y);
                }
                else
                    x = i;
                
                slots[i] = Instantiate(coinPlaceholder,
                                        StaticFunctions.Slerp(transform.GetChild(0).position - (xMax - 1) / 2f * spread * transform.right - y * spread * transform.up,
                                        transform.GetChild(0).position + (xMax - 1) / 2f * spread * transform.right - y * spread * transform.up,
                                        x / (xMax - 1f),
                                        transform.position - spread * y * transform.up),
                                        Quaternion.AngleAxis(90, Vector3.up),
                                        transform);
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

    public void DropCoins()
    {
        for (int i = 0; i < slotsFilled; i++)
        {
            filledSlots[i].GetComponent<Resource>().Drop();
        }
    }

    GameObject[] filledSlots;
    int slotsFilled = 0;


    [SerializeField] bool drawDebug;
    private void OnDrawGizmosSelected()
    {
        if (drawDebug)
        {
            Gizmos.color = Color.cyan;
            int y = 0;
            int x;
            int xMax = cost;
            string txt = "New Gizmos:\n";
            for (int i = 0; i < cost; i++)
            {
                if (cost > 4)
                {
                    x = i % ((cost + 1) / 2);
                    y = i / ((cost + 1) / 2);
                    xMax = cost % 2 == 0 ? cost / 2 : ((cost+1) / 2 - y);
                }
                else
                    x = i;
                txt += $"Y = {y}, X = {x}, XMax = {xMax}\n";
                                                                  /// coin center    +/- half the coins  * separation                   * lower it by the row
                Gizmos.DrawSphere(StaticFunctions.Slerp(transform.GetChild(0).position - (xMax - 1) / 2f * spread * transform.right - y * spread * transform.up, 
                                                        transform.GetChild(0).position + (xMax - 1) / 2f * spread * transform.right - y * spread * transform.up,
                                                        x / (xMax - 1f), 
                                                        transform.position - y * spread * transform.up),
                                                        .3f);
            }
            print(txt);
        }
    }
}
