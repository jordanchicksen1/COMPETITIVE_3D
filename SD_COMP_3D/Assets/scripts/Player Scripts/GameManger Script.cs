using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMangerScript : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> PointsUI;
    public List<TextMeshProUGUI> HealthText;

    private PlayerInputManager playerInputManager;
    public List<Transform> spawnPoints;
    public List<Transform> Players;
    [SerializeField]
    public List<Transform> midPoint;
    [SerializeField]
    private int Speed;
    [SerializeField]
    private List<GameObject> controlButtons;
    [SerializeField]
    private GameObject PodiumCamera;

    public GameObject ConfirmPanel;
    public GameObject ConfirmButton, Backbutton;
    public EventSystem eventSystem;

    public BombSpawner _bombSpawner;
    private bool CheckForEndGame;
    public Transform LastPlayer;
    [SerializeField]
    private Transform WinPodium;
    public GameObject WinCamera;
    [SerializeField]
    private GameObject RestartButton;
    public GameObject WinCanvas;
    [SerializeField] string MainMenu;
    private bool canRunWinRoutine;

    public enum GameType { DeathMatch, KingOfTheLedge }
    [SerializeField] private GameType gameType;

    private void Start()
    {
        playerInputManager = FindFirstObjectByType<PlayerInputManager>();

        for (int i = 0; i < PointsUI.Count; i++)
        {
            PointsUI[i].gameObject.SetActive(false);
        }

    }

    private void Update()
    {
        if (playerInputManager != null && playerInputManager.playerCount > 0)
        {
            PointsUI[playerInputManager.playerCount - 1].SetActive(true);
        }
    }

    public void StartGame()
    {
       switch(gameType)
        {
            case GameType.DeathMatch:
               
                break;
            case GameType.KingOfTheLedge:
                for (int i = 0; i < Players.Count; i++)
                {

                    if (Players[i] != null)
                    {
                        PlayerController3D playerScript = Players[i].GetComponent<PlayerController3D>();
                        playerScript.speed = Speed;
                        CheckForEndGame = true;
                    }
                }
                break;
        }

        PodiumCamera.SetActive(false);

        for (int r = 0; r < midPoint.Count; r++)
        {
            if (Players[r] != null)
            {
                Players[r].position = midPoint[r].position;
            }
        }
        ConfirmPanel.SetActive(false);
        Time.timeScale = 1;
        playerInputManager.DisableJoining();

        _bombSpawner.StartSpawning();

    }

    public void ShowConfirmPanel()
    {
        ConfirmPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(ConfirmButton);
        for (int j = 0; j < controlButtons.Count; j++)
        {
            controlButtons[j].SetActive(false);
        }
        Time.timeScale = 0;
    }

    public void HideConfirmPanel()
    {
        ConfirmPanel.SetActive(false);
        eventSystem.SetSelectedGameObject(Backbutton);

        for (int j = 0; j < controlButtons.Count; j++)
        {
            controlButtons[j].SetActive(true);
        }
        Time.timeScale = 1;

    }

    private void FixedUpdate()
    {
        if (playerInputManager.playerCount == 1 && CheckForEndGame)
        {
            if (!canRunWinRoutine)
            {
                StartCoroutine(GotoWinPodium());
                canRunWinRoutine = true;
            }
        }
    }

    public void RestartGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenu);
    }

    IEnumerator GotoWinPodium()
    {
        yield return new WaitForSeconds(5);

       if (LastPlayer != null )
        {
            LastPlayer.position = WinPodium.transform.position;
            WinCamera.SetActive(true);

            PlayerController3D playerScript = LastPlayer.GetComponent<PlayerController3D>();
            Rigidbody rb = LastPlayer.GetComponent<Rigidbody>();
            rb.useGravity = false;
            playerScript.speed = 0;
            WinCanvas.SetActive(true);
            eventSystem.SetSelectedGameObject(RestartButton);
        }
       

    }
}
