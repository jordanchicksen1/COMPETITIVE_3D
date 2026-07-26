using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    private float maxHealth = 100f;

    [SerializeField]
    private TextMeshProUGUI _healthText;
    private PlayerInput _playerInput;
    private GameMangerScript _managerScript;

    [SerializeField]
    private AnimationManager animationScript;
    [SerializeField]
    private PlayerController3D playerScript;
    [SerializeField]
    private TargetGroupAutoRegister RegisterScript;

    private bool isDead = false;
    private float lastDisplayedHealth = -1;

    private void Start()
    {
        _managerScript = FindFirstObjectByType<GameMangerScript>();
        _playerInput = GetComponent<PlayerInput>();
        _healthText = _managerScript.HealthText[_playerInput.playerIndex];
        RegisterScript = GetComponent<TargetGroupAutoRegister>();
        animationScript = GetComponent<AnimationManager>();
        playerScript = GetComponent<PlayerController3D>();

        lastDisplayedHealth = health;
        _healthText.text = health.ToString();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;
        health = Mathf.Round(health);
        health = Mathf.Max(health, 0f);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} died!");

        // Disable player input so they can't move/jump/throw
        _playerInput.enabled = false;

        // Stop animations
        animationScript.StopFacialReactions();
        animationScript.PlayDying();

        StartCoroutine(PlayerDeath());
    }

    private void Update()
    {
        // Only update text if health changed
        if (health != lastDisplayedHealth)
        {
            _healthText.text = health.ToString();
            lastDisplayedHealth = health;
        }
    }

    IEnumerator PlayerDeath()
    {
        yield return new WaitForSeconds(3);
        _healthText.text = "X";

        if (RegisterScript != null)
        {
            RegisterScript.RemovePlayer();
        }
        else
        {
            Debug.LogWarning("RegisterScript not found on " + gameObject.name);
        }
    }
}