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

    //Capture The Flag
    [Header("Capture The Flag")]
    [Tooltip("Layer the flag pickup object lives on, checked alongside Interact.")]
    public LayerMask FlagLayer;
    [Tooltip("Where the flag attaches while this player is carrying it.")]
    [SerializeField]
    private Transform FlagHoldPosition;
    [Tooltip("Where this player respawns after dying.")]
    public Transform SpawnPoint;
    [Tooltip("Optional team id, used by FlagCaptureZone to tell friendly vs enemy flag.")]
    public int TeamId = 0;

    private Flag currentFlagInRange;
    private Flag heldFlag;
    public bool HasFlag => heldFlag != null;
    public Flag CarriedFlag => heldFlag;

    //Health / Death
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    public bool IsDead { get; private set; }

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
        currentHealth = maxHealth;
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
            // Prefer picking up a bomb if one is in range and hands are free.
            if (currentBomb != null && heldWeapon == null)
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
            // Otherwise, pick up the flag if one is in range and not already carried.
            else if (currentFlagInRange != null && heldFlag == null && currentFlagInRange.State != Flag.FlagState.Carried)
            {
                PickUpFlag(currentFlagInRange);
            }
        }
    }

    private void PickUpFlag(Flag flag)
    {
        Transform holdPoint = FlagHoldPosition != null ? FlagHoldPosition : HoldingPosition;
        flag.PickUp(holdPoint, gameObject);
        heldFlag = flag;
    }

    // Drops the carried flag in place, e.g. if manually released without dying.
    public void DropFlag()
    {
        if (heldFlag == null) return;

        heldFlag.Drop(transform.position);
        heldFlag = null;
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

        CheckForFlagInteraction();
    }

    // Separate proximity check for the flag, on its own layer, so bombs and
    // the flag can both be highlighted/interacted with independently.
    void CheckForFlagInteraction()
    {
        float interactionRange = 2f;
        Collider[] flagColliders = Physics.OverlapSphere(transform.position, interactionRange, FlagLayer);

        GameObject closestFlagObj = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in flagColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFlagObj = collider.gameObject;
            }
        }

        Flag closestFlag = closestFlagObj != null ? closestFlagObj.GetComponent<Flag>() : null;

        if (closestFlag != currentFlagInRange)
        {
            if (currentFlagInRange != null)
            {
                Outline oldOutline = currentFlagInRange.GetComponent<Outline>();
                if (oldOutline != null)
                {
                    Destroy(oldOutline);
                }
            }

            currentFlagInRange = closestFlag;

            if (currentFlagInRange != null && currentFlagInRange.State != Flag.FlagState.Carried)
            {
                Outline newOutline = currentFlagInRange.GetComponent<Outline>();
                if (newOutline == null)
                {
                    newOutline = currentFlagInRange.gameObject.AddComponent<Outline>();
                    newOutline.OutlineWidth = 5;
                    newOutline.OutlineColor = Color.white;
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

    // --- Health / Death / Respawn -------------------------------------

    // Call this from your bomb explosion / damage code (e.g. BombManager)
    // whenever this player should take damage.
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Flag drops right where the player died.
        if (heldFlag != null)
        {
            heldFlag.Drop(transform.position);
            heldFlag = null;
        }

        // Don't let a held bomb vanish/teleport with the player on respawn.
        if (heldWeapon != null)
        {
            heldWeapon.transform.parent = null;
            heldWeapon = null;
        }

        Respawn();
    }

    private void Respawn()
    {
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (SpawnPoint != null)
        {
            rb.position = SpawnPoint.position;
            transform.rotation = SpawnPoint.rotation;
        }

        if (animManager != null)
        {
            animManager.PlayIdle();
        }

        IsDead = false;
    }
}