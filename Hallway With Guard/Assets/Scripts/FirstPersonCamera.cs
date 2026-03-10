using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [Header("Look Settings")]
    public InputActionReference lookAction;
    public float mouseSensitivity = 100f;
    public float controllerSensitivity = 250f;

    float pitch = 0f; // up/down
    Transform playerBody;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerBody = transform.parent;
        lookAction?.action.Enable();
    }

    void Update()
    {
        if (lookAction == null) return;

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        bool usingController = Gamepad.current != null && lookInput.magnitude > 0.01f;
        float sens = usingController ? controllerSensitivity : mouseSensitivity;

        float mouseX = lookInput.x * sens * Time.deltaTime;
        float mouseY = lookInput.y * sens * Time.deltaTime;

        // yaw -> rotate player body
        playerBody.Rotate(Vector3.up * mouseX);

        // pitch -> rotate camera up/down
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}