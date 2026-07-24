using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(Rigidbody))]
public class FlagPlayerController : MonoBehaviour
{
    [SerializeField]
    private Vector3 moveInput;

    private Rigidbody rb;
    private PlayerInput playerInput;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public float SpeedMultiplier;

    //Interactions
    private GameObject InteractableObject;
    public LayerMask Interact;
    [SerializeField]


    //Attack
    private GameObject heldWeapon;
    [SerializeField]
    private Transform HoldingPosition;
    [SerializeField]
    private Transform HoldParent;

    //PLayer Animations
    [Header("Animations")]
    [SerializeField]
    private AnimationManager animManager;
    [SerializeField]
    public Transform rayPoint;

    [SerializeField]
    private Color outlineColour_;
    [SerializeField]
    private List<Color> playerColours;
    private GameObject currentBomb;
    [SerializeField]
    private float throwForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        playerInput = GetComponent<PlayerInput>();
        outlineColour_ = playerColours[playerInput.playerIndex];

    }



    // MOVEMENT
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y);
    }

    //Inventory System
    void Update()
    {
        UpdateMovementAnimation();
    }

    // Drives Run / HoldRun / Idle every frame based on movement input and
    // whether a bomb is currently held. Skipped while a one-shot animation
    // (jump/throw) is playing so it doesn't get cut off early.
    private void UpdateMovementAnimation()
    {
        if (animManager == null || animManager.IsBusy)
        {
            return;
        }

        bool isMoving = new Vector3(moveInput.x, 0f, moveInput.z).sqrMagnitude > 0.001f;
        bool isHoldingWeapon = heldWeapon != null;

        if (isMoving && isHoldingWeapon)
        {
            animManager.PlayHoldRun();
        }
        else if (isMoving)
        {
            animManager.PlayRun();
        }
        else if (isHoldingWeapon && !isMoving)
        {
            animManager.PlayHoldIdle();
        }
        else
        {
            animManager.PlayIdle();

        }
    }



    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            if (animManager != null)
            {
                animManager.PlayJump();
            }
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            speed = speed * SpeedMultiplier;
        }
        else if (context.canceled)
        {
            speed = speed / SpeedMultiplier;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            if (currentBomb != null)
            {
                heldWeapon = currentBomb;
                heldWeapon.transform.position = HoldingPosition.position;
                heldWeapon.transform.parent = HoldingPosition;
                BombManager bombScript = heldWeapon.GetComponent<BombManager>();
                if (bombScript != null)
                {
                    bombScript.canCheckCollisions = true;
                }

            }
        }
    }

    void CheckForInteraction()
    {
        float interactionRange = 2f; // Proximity range (was raycast distance)

        // Find all colliders in range on the Interact layer
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, Interact);

        GameObject closestBomb = null;
        float closestDistance = float.MaxValue;

        // Find the closest bomb
        foreach (Collider collider in colliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBomb = collider.gameObject;
            }
        }

        // Update currentBomb if it changed
        if (closestBomb != currentBomb)
        {
            // Remove outline from old bomb
            if (currentBomb != null)
            {
                Outline outline = currentBomb.GetComponent<Outline>();
                if (outline != null)
                {
                    Destroy(outline);
                }
            }

            // Add outline to new bomb
            currentBomb = closestBomb;
            if (currentBomb != null)
            {
                Outline currentOutline = currentBomb.GetComponent<Outline>();
                if (currentOutline == null)
                {
                    currentBomb.AddComponent<Outline>();
                    currentOutline = currentBomb.GetComponent<Outline>();
                    currentOutline.OutlineWidth = 5;
                    AssignColour();
                }
            }
        }
    }

    public void AssignColour()
    {

        if (currentBomb != null)
        {
            Outline outline = currentBomb.gameObject.GetComponent<Outline>();
            switch (playerInput.playerIndex)
            {
                case 0:
                    outline.OutlineColor = Color.green;
                    break;

                case 1:
                    outline.OutlineColor = Color.red;
                    break;

                case 2:
                    outline.OutlineColor = Color.blue;
                    break;

                case 3:
                    outline.OutlineColor = Color.yellow;
                    break;

                default:
                    outline.OutlineColor = Color.white;
                    break;
            }
        }
    }



    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            if (heldWeapon != null)
            {
                // Get existing rigidbody instead of adding a new one every throw
                Rigidbody rb = heldWeapon.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = heldWeapon.AddComponent<Rigidbody>();
                }

                // Use Impulse for a more "thrown" feel (instant burst)
                rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

                BombManager bombSCript = heldWeapon.GetComponent<BombManager>();
                if (bombSCript != null)
                {
                    bombSCript.ActivateBomb();
                }

                heldWeapon.transform.parent = null;
                heldWeapon = null;

                if (animManager != null)
                {
                    animManager.PlayThrow();
                }
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.z);

        // Move relative to world (NOT current rotation)
        rb.MovePosition(rb.position + inputDir * speed * Time.fixedDeltaTime);

        // Rotate ONLY when moving
        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.fixedDeltaTime
            );
        }

        CheckForInteraction();
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}