using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference lookAction;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.6f;
    public float gravity = -30f;

    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;

    Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // find the camera in children
        cameraTransform = GetComponentInChildren<Camera>()?.transform;

        moveAction?.action.Enable();
        jumpAction?.action.Enable();
        lookAction?.action.Enable();
    }

    void Update()
    {
        // grounded check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // stick to ground

        // read movement input
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // movement relative to camera forward/right
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * input.y + camRight * input.x;
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // jump
        if (jumpAction.action.triggered && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}