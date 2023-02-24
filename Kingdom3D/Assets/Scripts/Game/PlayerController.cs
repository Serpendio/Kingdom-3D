using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rig;

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
    }

    public bool isPaying;
    public void PayResource(InputAction.CallbackContext context)
    {
        if (context.performed && currentNearest != -1)
        {
            isPaying = true;
            StartCoroutine(PayInteractable());
        }
        if (context.canceled)
        {
            isPaying = false;

            // set to nearest interactable
        }
    }

    private IEnumerator PayInteractable()
    {
        while (isPaying)
        {
            nearbyInteractables[currentNearest].AddCoin(Instantiate(ObjectReferences.Instance.coin, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up)));
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void DropCoin(InputAction.CallbackContext context)
    {
        if (context.performed) Instantiate(ObjectReferences.Instance.coin, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
    }

    void FindNearestInteractable()
    {
        currentNearest = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < nearbyInteractables.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, nearbyInteractables[i].transform.position);
            if (dist < closestDistance)
            {
                currentNearest = i;
                closestDistance = dist;
            }
        }
    }

    readonly List<Interactable> nearbyInteractables = new();
    int currentNearest = -1;
    public void UpdateInteractables(bool beganOverlap, Interactable interactable)
    {
        if (beganOverlap)
        {
            nearbyInteractables.Add(interactable);

            if (!isPaying)
            {
                if (currentNearest != -1)
                {
                    nearbyInteractables[currentNearest].SetCostVisibility(false);
                }
                FindNearestInteractable();
                nearbyInteractables[currentNearest].SetCostVisibility(true);
            }
        }
        else
        {
            if (nearbyInteractables.IndexOf(interactable) == currentNearest)
            {
                isPaying = false;
                nearbyInteractables.Remove(interactable);

                if (nearbyInteractables.Count == 0)
                    currentNearest = -1;
                else
                {
                    FindNearestInteractable();
                    nearbyInteractables[currentNearest].SetCostVisibility(true);
                }
            }
            else { nearbyInteractables.Remove(interactable); }
        }
    }

    public void EnableConsole(InputAction.CallbackContext context)
    {
        if (context.performed && RuntimeConsole.Instance != null)
        {
            GetComponent<PlayerInput>().DeactivateInput();
            RuntimeConsole.Instance.ShowConsole();
        }
    }

    private void FixedUpdate()
    {
        rig.velocity = (transform.forward * movement.y + transform.right * movement.x) * (isSprinting ? sprintSpeed : moveSpeed) + Vector3.up * rig.velocity.y;
    }
}
