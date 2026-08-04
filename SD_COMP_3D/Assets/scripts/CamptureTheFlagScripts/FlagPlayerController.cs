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

    [Header("Dash")]
    [Tooltip("How fast the player moves during a dash.")]
    public float dashSpeed = 20f;
    [Tooltip("How long the dash burst lasts, in seconds.")]
    public float dashDuration = 0.15f;
    [Tooltip("Time between dashes, in seconds.")]
    public float dashCooldown = 1.5f;

    private bool isDashing;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private Vector3 dashDirection;

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

    [Header("Head Colour")]
    public Renderer Head_Material_Renderer;
    [SerializeField]
    private List<Material> HeadMaterials;

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

    [Tooltip("Layer other players are on. Used to detect nearby flag carriers to steal from.")]
    public LayerMask PlayerLayer;

    private Flag currentFlagInRange;
    private Flag heldFlag;
    private FlagPlayerController currentStealTarget;
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
        AssignHeadColour();

        AssignSpawnPointAndTeam();
    }

    void AssignHeadColour()
    {
        if (playerInput.playerIndex >= 0 && playerInput.playerIndex < HeadMaterials.Count)
        {
            Material[] materials = Head_Material_Renderer.materials;
            materials[1] = HeadMaterials[playerInput.playerIndex];
            Head_Material_Renderer.materials = materials;
        }
    }

    private void AssignSpawnPointAndTeam()
    {
        int joinOrder = playerInput.playerIndex + 1;
        TeamId = joinOrder;

        GameObject spawnObj = GameObject.FindWithTag($"P{joinOrder}Spawn");
        if (spawnObj != null)
        {
            SpawnPoint = spawnObj.transform;
            rb.position = SpawnPoint.position;
            transform.rotation = SpawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning($"FlagPlayerController: no object tagged 'P{joinOrder}Spawn' found in the scene.");
        }
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

        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }
    }

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

    public void PlayJumpAnimation()
    {
        if (animManager != null)
        {
            animManager.PlayJump();
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

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isDashing || dashCooldownRemaining > 0f) return;

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.z);
        dashDirection = inputDir.sqrMagnitude > 0.001f ? inputDir.normalized : transform.forward;

        isDashing = true;
        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            if (currentBomb != null && heldWeapon == null && currentBomb.GetComponent<Flag>() == null)
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
            else if (currentFlagInRange != null && heldFlag == null && currentFlagInRange.State != Flag.FlagState.Carried)
            {
                PickUpFlag(currentFlagInRange);
            }
            else if (currentStealTarget != null && heldFlag == null && currentStealTarget.HasFlag)
            {
                StealFlagFrom(currentStealTarget);
            }
        }
    }

    private void PickUpFlag(Flag flag)
    {
        Transform holdPoint = FlagHoldPosition != null ? FlagHoldPosition : HoldingPosition;
        flag.PickUp(holdPoint, gameObject);
        heldFlag = flag;
    }

    private void StealFlagFrom(FlagPlayerController victim)
    {
        Flag stolenFlag = victim.CarriedFlag;
        if (stolenFlag == null) return;

        if (stolenFlag.IsProtectedFromSteal) return;

        victim.ClearHeldFlag();
        PickUpFlag(stolenFlag);
    }

    public void DropFlag()
    {
        if (heldFlag == null) return;

        heldFlag.Drop(transform.position);
        heldFlag = null;
    }

    public void ClearHeldFlag()
    {
        heldFlag = null;
    }

    void CheckForInteraction()
    {
        float interactionRange = 2f;

        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, Interact);

        GameObject closestBomb = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<Flag>() != null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBomb = collider.gameObject;
            }
        }

        if (closestBomb != currentBomb)
        {
            if (currentBomb != null)
            {
                Outline outline = currentBomb.GetComponent<Outline>();
                if (outline != null)
                {
                    Destroy(outline);
                }
            }

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
        CheckForStealTarget();
    }

    void CheckForStealTarget()
    {
        float interactionRange = 2f;
        Collider[] playerColliders = Physics.OverlapSphere(transform.position, interactionRange, PlayerLayer);

        FlagPlayerController closestCarrier = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in playerColliders)
        {
            FlagPlayerController other = collider.GetComponentInParent<FlagPlayerController>();
            if (other == null || other == this || !other.HasFlag)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCarrier = other;
            }
        }

        currentStealTarget = closestCarrier;
    }

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
                Rigidbody rb = heldWeapon.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = heldWeapon.AddComponent<Rigidbody>();
                }

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
        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);

            dashTimeRemaining -= Time.fixedDeltaTime;
            if (dashTimeRemaining <= 0f)
            {
                isDashing = false;
            }

            CheckForInteraction();
            return;
        }

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.z);

        rb.MovePosition(rb.position + inputDir * speed * Time.fixedDeltaTime);

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

        if (heldFlag != null)
        {
            heldFlag.Drop(transform.position);
            heldFlag = null;
        }

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