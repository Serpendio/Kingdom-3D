using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float sensitivityY;
    [SerializeField] float sensitivityX;
    float rotation;

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * Time.deltaTime;

        transform.parent.Rotate(transform.parent.up, mouseDelta.x * sensitivityX);

        float rotationAngle = rotation - mouseDelta.y * sensitivityY;
        float targetAngle = Mathf.Clamp(rotationAngle, -70, 70);
        float currentAngle = Mathf.Clamp(rotationAngle, -90, 90);
        rotation = Mathf.Lerp(currentAngle, targetAngle, 4f * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(Vector3.right * rotation);
    }
}
