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
    private TextMeshProUGUI _PointsText;
    public int _playerPoints;
    private PlayerInput _playerInput;
    private GameMangerScript _managerScript;
    private PlayerInputManager _playerInputManager;

    [SerializeField]
    private AnimationManager animationScript;
    [SerializeField]
    private PlayerController3D playerScript;
    [SerializeField]
    private TargetGroupAutoRegister RegisterScript;

    private bool isDead = false;
    private float lastDisplayedHealth = -1;


    public enum GameType { DeathMatch, KingOfTheLedge }
    [SerializeField] private GameType gameType;


    private void Start()
    {
        _managerScript = FindFirstObjectByType<GameMangerScript>();
        _playerInput = GetComponent<PlayerInput>();
        switch (gameType)
        {
            case GameType.DeathMatch:
                _PointsText = _managerScript.HealthText[_playerInput.playerIndex];
                _PointsText.text = _playerPoints.ToString();
                break;
            case GameType.KingOfTheLedge:
                _healthText = _managerScript.HealthText[_playerInput.playerIndex];
                _healthText.text = health.ToString();
                playerScript = GetComponent<PlayerController3D>();
                break;
        }
        RegisterScript = GetComponent<TargetGroupAutoRegister>();
        animationScript = GetComponent<AnimationManager>();
        _playerInputManager = FindAnyObjectByType<PlayerInputManager>();

        lastDisplayedHealth = health;

        SpawmPlayer();
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

    public void Die()
    {
        switch (gameType)
        {
            case GameType.DeathMatch:
                _playerInput.enabled = false;
                StartCoroutine(RespawnTimer());
                animationScript.PlayDying();
                break;

            case GameType.KingOfTheLedge:
                isDead = true;
                Debug.Log($"{gameObject.name} died!");

                // Disable player input so they can't move/jump/throw
                _playerInput.enabled = false;

                // Stop animations
                animationScript.StopFacialReactions();
                animationScript.PlayDying();

                StartCoroutine(PlayerDeath());
                break;
        }
    }

    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(4);
        _playerInput.enabled = true;
        Respawn();
    }

    public void Respawn()
    {
        isDead = false;
        health = maxHealth;

        _playerInput.enabled = true;
        transform.position = _managerScript.midPoint[Random.Range(0, _managerScript.midPoint.Count)].position;

        animationScript.PlayIdle();
    }

    private void Update()
    {
        switch (gameType)
        {
            case GameType.KingOfTheLedge:
                // Only update text if health changed
                if (health != lastDisplayedHealth)
                {
                    _healthText.text = health.ToString();
                    lastDisplayedHealth = health;
                }
                break;
        }
    }

    IEnumerator PlayerDeath()
    {
        yield return new WaitForSeconds(5);
        _healthText.text = "X";

        if (RegisterScript != null)
        {
            RegisterScript.RemovePlayer();
            transform.tag = null;
        }
        else
        {
            Debug.LogWarning("RegisterScript not found on " + gameObject.name);
        }
    }

    public void SpawmPlayer()
    {
        switch (gameType)
        {
            case GameType.DeathMatch:
                transform.position = _managerScript.spawnPoints[_playerInput.playerIndex].position;

                _managerScript.Players[_playerInput.playerIndex] = transform;
                break;
            case GameType.KingOfTheLedge:
                transform.position = _managerScript.spawnPoints[_playerInput.playerIndex].position;
                playerScript.speed = 0;

                _managerScript.Players[_playerInput.playerIndex] = transform;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("KillBox"))
        {
            Die();
            PlayerDeath();
        }
    }

    private void FixedUpdate()
    {
        if (_playerInputManager.playerCount == 1 && !isDead)
        {
            _managerScript.LastPlayer = transform;
        }
    }
}