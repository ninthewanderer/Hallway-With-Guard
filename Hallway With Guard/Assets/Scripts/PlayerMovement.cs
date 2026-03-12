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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public float footstepInterval = 0.45f;

    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;
    bool wasGrounded;

    Transform cameraTransform;
    float footstepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        cameraTransform = GetComponentInChildren<Camera>()?.transform;

        moveAction?.action.Enable();
        jumpAction?.action.Enable();
        lookAction?.action.Enable();
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * input.y + camRight * input.x;
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        bool isMoving = input.magnitude > 0.1f;

        if (isGrounded && isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                audioSource.PlayOneShot(walkSound);
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        if (jumpAction.action.triggered && isGrounded)
        {
            audioSource.PlayOneShot(jumpSound);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (!wasGrounded && isGrounded)
        {
            audioSource.PlayOneShot(landSound);
        }
    }
}