using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rig;

    [SerializeField] GameObject coin;
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float sprintSpeed = 15;
    [SerializeField] float maxStamina = 3;
    [SerializeField] float staminaRecovery = 0.25f;
    
    bool isSprinting;
    float currentStamina;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (movement == Vector2.zero) // includes approximation
        {
            currentStamina = Mathf.Clamp(currentStamina + staminaRecovery * Time.deltaTime, 0, maxStamina);
        }
        else if (isSprinting)
        {
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0)
                isSprinting = false;
        }
    }

    Vector2 movement;
    public void Move(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
        print(context);
    }

    public void PayResource(InputAction.CallbackContext context)
    {
        if (context.performed) Instantiate(coin, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
    }

    List<Interactable> nearbyInteractables = new();
    int currentNearest = -1;
    public void UpdateInteractables(bool beganOverlap, Interactable interactable)
    {
        void findNearest() 
        {
            float closestDistance = float.MaxValue;
            for (int i = 0; i < nearbyInteractables.Count; i++)
            {
                if (Vector3.Distance(transform.position, nearbyInteractables[i].transform.position) < closestDistance)
                    currentNearest = i;
            }
        }

        if (beganOverlap)
        {
            nearbyInteractables.Add(interactable);
            if (currentNearest != -1)
            {
                nearbyInteractables[currentNearest].SetCostVisibility(false);
            }
            findNearest();
            nearbyInteractables[currentNearest].SetCostVisibility(true);
        }
        else
        {
            if (nearbyInteractables.IndexOf(interactable) == currentNearest)
            {
                nearbyInteractables.Remove(interactable);
                if (nearbyInteractables.Count == 0)
                    currentNearest = -1;
                else
                {
                    findNearest();
                    nearbyInteractables[currentNearest].SetCostVisibility(true);
                }
            }
            else { nearbyInteractables.Remove(interactable); }
        }
    }

    private void FixedUpdate()
    {
        rig.velocity = (transform.forward * movement.y + transform.right * movement.x) * (isSprinting ? sprintSpeed : moveSpeed) + Vector3.up * rig.velocity.y;
    }
}
