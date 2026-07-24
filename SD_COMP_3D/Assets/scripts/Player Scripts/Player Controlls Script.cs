using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController3D : MonoBehaviour
{
    [SerializeField]
    private Vector3 moveInput;
    private Vector2 lookInput;

    private Rigidbody rb;
    private PlayerInput playerInput;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public float SpeedMultiplier;
    private float RunSpeed;
    [Header("Look")]



    //Interactions
    private GameObject InteractableObject;
    public LayerMask Interact;
    [SerializeField]
    private Transform RayPoint;


    //Attack
    [SerializeField]
    private bool isChargingWeapon;
    [SerializeField]
    private float attackPower;
    [SerializeField]
    private GameObject heldWeapon;
    [SerializeField]
    private Transform HoldingPosition;
    [SerializeField]
    private Transform HoldParent;
    public LayerMask EnemyLayer;
    [SerializeField]
    private int AimDistance;
    public GameObject EnemyTarget;
    [SerializeField]
    private Transform AimPoint;

    //Player Assortment Manager
    [SerializeField]
    private MultiplayerEventSystem eventSystem;
    [SerializeField] private GameObject PauseFirstSelect, InventoryFirstSelect;

    //PLayer Animations
    [Header("Animations")]
    [SerializeField]
    private Animator playerAnimations;
    [SerializeField]
    private AnimationManager animManager;
    private bool isJumping;
    [SerializeField]
    private List<string> AnimationBools;
    public Transform rayPoint;

    [SerializeField]
    private Color outlineColour_;
    [SerializeField]
    private List<Color> playerColours;
    private GameObject currentBomb;

    [Header("Knockback")]
    [SerializeField]
    private float knockbackDrag = 5f; // higher = knockback fades out faster
    private Vector3 knockbackVelocity;
    [SerializeField]
    private float throwForce;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Called by external systems (e.g. BombManager) to push the player.
    // Needed because normal movement uses MovePosition every FixedUpdate,
    // which would otherwise instantly cancel out any physics force applied
    // directly to the Rigidbody (like AddExplosionForce).
    public void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity += force;
    }

    void Start()
    {
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.defaultActionMap = "UI";
        Cursor.lockState = CursorLockMode.None;

        RunSpeed = speed * SpeedMultiplier;

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
        else
        {
            animManager.PlayIdle();
        }
    }


    // LOOK
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }


    // Pause/Play

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
                    // Collisions should NOT be armed while the bomb is being
                    // carried, only once it's actually thrown (see OnThrow).
                    bombScript.canCheckCollisions = false;
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


    public void OnGameSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
            SceneManager.LoadScene("GameSelect");
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
                rb.AddForce(HoldingPosition.forward * throwForce, ForceMode.Impulse);

                BombManager bombSCript = heldWeapon.GetComponent<BombManager>();
                if (bombSCript != null)
                {
                    // Arm collision detection now that it's actually in flight.
                    bombSCript.canCheckCollisions = true;
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

        // Decay any active knockback so it fades out rather than persisting forever
        if (knockbackVelocity.sqrMagnitude > 0.01f)
        {
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDrag * Time.fixedDeltaTime);
        }
        else
        {
            knockbackVelocity = Vector3.zero;
        }

        // Move relative to world (NOT current rotation), blended with any knockback
        rb.MovePosition(rb.position + (inputDir * speed + knockbackVelocity) * Time.fixedDeltaTime);

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