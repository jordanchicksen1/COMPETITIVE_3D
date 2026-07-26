using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameMangerScript : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> PointsUI;
    public List<TextMeshProUGUI> HealthText;

    private PlayerInputManager playerInputManager;
    public List<Transform> spawnPoints;
    public List<Transform> Players;
    [SerializeField]
    private List<Transform> midPoint;
    [SerializeField]
    private int Speed;
    [SerializeField]
    private List<GameObject> controlButtons;
    [SerializeField]
    private GameObject PodiumCamera;

    public GameObject ConfirmPanel;
    public GameObject ConfirmButton, Backbutton;
    public EventSystem eventSystem;

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
        for (int i = 0; i < Players.Count; i++)
        {

            if (Players[i] != null)
            {
                PlayerController3D playerScript = Players[i].GetComponent<PlayerController3D>();
                playerScript.speed = Speed;
            }
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
}
