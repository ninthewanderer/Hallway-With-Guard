using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;            // assign the body that turns with yaw
    public InputActionReference lookAction;  // reference to Player/Look action

    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public float controllerSensitivity = 2f;

    float xRot;
    float yRot;

    void Start()
    {
        // lock cursor for FPS feel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (lookAction == null)
            return;

        // read input
        Vector2 input = lookAction.action.ReadValue<Vector2>();

        // detect controller vs mouse
        bool usingController = Gamepad.current != null && input.magnitude > 0.01f;

        float sens = usingController ? controllerSensitivity : mouseSensitivity;

        // apply
        float dx = input.x * sens * Time.deltaTime;
        float dy = input.y * sens * Time.deltaTime;

        yRot += dx;
        xRot -= dy;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        // rotate camera
        transform.localRotation = Quaternion.Euler(xRot, yRot, 0);

        // rotate body
        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRot, 0);
    }
}