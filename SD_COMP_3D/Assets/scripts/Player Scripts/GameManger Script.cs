using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMangerScript : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> PointsUI;
    public List<TextMeshProUGUI> HealthText;

    private PlayerInputManager playerInputManager;


    private void Start()
    {
        playerInputManager = FindFirstObjectByType<PlayerInputManager>();
        
        for(int i = 0; i < PointsUI.Count; i++)
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
}
