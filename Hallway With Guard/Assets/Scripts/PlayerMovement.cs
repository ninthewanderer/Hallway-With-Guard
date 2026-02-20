using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;             // yaw-only transform (player body)
    public InputActionReference moveAction;   // Player/Move (Vector2)
    public InputActionReference jumpAction;   // Player/Jump (Button)

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float groundedStick = -2f;         // small downward force when grounded

    CharacterController controller;
    Vector3 velocity;
    bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // ensure actions are enabled
        moveAction?.action?.Enable();
        jumpAction?.action?.Enable();
    }

    void Update()
    {
        // log messages for debugging
        if (orientation == null)
        {
            Debug.LogError("[PlayerMovement] orientation not assigned!");
            return;
        }
        if (moveAction == null || moveAction.action == null)
        {
            Debug.LogError("[PlayerMovement] moveAction not assigned!");
            return;
        }
        if (jumpAction == null || jumpAction.action == null)
        {
            Debug.LogError("[PlayerMovement] jumpAction not assigned!");
            return;
        }

        // grounded check (CharacterController provides this)
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            // small downward constant so controller stays anchored
            velocity.y = groundedStick;
        }

        // read input (Vector2)
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // build movement using orientation (yaw-only ideally)
        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;

        // zero out vertical component to keep movement horizontal
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x);
        if (move.sqrMagnitude > 1f) move.Normalize(); // avoid faster diagonal

        // perform horizontal movement
        controller.Move(move * moveSpeed * Time.deltaTime);

        // jump (use triggered to catch button presses)
        if (jumpAction.action.triggered && isGrounded)
        {
            // v = sqrt(2 * g * h) but gravity is negative, so:
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // apply gravity
        velocity.y += gravity * Time.deltaTime;

        // apply vertical movement
        controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);

        wasGrounded = isGrounded;
    }
}