﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.TankGame.Scripts;
using Mirror;
using Mirror.Examples.Tanks;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TankManager : NetworkBehaviour
{
    public GameObject MaxHealthBar;
    public GameObject HealthBar;
    public GameObject GoToMenuButton;
    public Text ScoreTextLabel;
    public GameObject GameOverPanel;
    public Text WinnerNameText;
    public Text TimerText;

    private bool IsGameReady;
    private TankPlayerController LocalPlayer;
    private bool IsGameOver;

    public List<TankPlayerController> players = new List<TankPlayerController>();
    private float timer = 15f;
    private float timerTime = 0;
    private bool gameTimerEnded;

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.singleton.isNetworkActive)
        {
            GameReadyCheck();
            GameOverCheck();

            if (LocalPlayer == null)
            {
                FindLocalTank();
            }
            else
            {
                UpdateStats();
            }

            BackToRoomCheck();

        }
        else
        {
            //Cleanup state once network goes offline
            IsGameReady = false;
            LocalPlayer = null;
        }
    }

    void BackToRoomCheck()
    {
        if (!IsGameOver)
            return;
        if (timerTime <= 0)
            timerTime = Time.realtimeSinceStartup;
        gameTimerEnded = Time.realtimeSinceStartup - timerTime >= timer;
#if UNITY_SERVER || UNITY_EDITOR
        if (gameTimerEnded && IsGameOver)
            GoBackToRoom();
        Debug.Log("Time calculated is : " + ((int)timerTime - (int)Time.realtimeSinceStartup));
#endif
    }

    void GameReadyCheck()
    {
        if (!IsGameReady)
        {
            //Look for connections that are not in the player list
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
            {
                TankPlayerController comp = kvp.Value.GetComponent<TankPlayerController>();

                //Add if new
                if (comp != null && !players.Contains(comp))
                {
                    players.Add(comp);
                }
            }
            HealthBar.SetActive(true);
            ScoreTextLabel.gameObject.SetActive(true);
        }
    }

    void GameOverCheck()
    {
        //Cant win a game you play by yourself. But you can still use this example for testing network/movement
        if (players.Count == 1)
            return;

        int alivePlayerCount = 0;
        foreach (TankPlayerController tank in players)
        {
            if (!tank.isDead)
            {
                alivePlayerCount++;
                WinnerNameText.text = tank.playerName;
            }
            if (tank.isDead && tank.gameObject.activeInHierarchy)
                tank.gameObject.SetActive(false);
        }

        if (alivePlayerCount == 1)
        {
            IsGameOver = true;
            GameOverPanel.SetActive(true);
        }
    }

    private void UpdateStats()
    {
        var healthBar = HealthBar.GetComponent<RectTransform>();
        healthBar.sizeDelta = new Vector2(LocalPlayer.health / 100 * MaxHealthBar.GetComponent<RectTransform>().sizeDelta.x,
            healthBar.sizeDelta.y);
        ScoreTextLabel.text = "Score : " + LocalPlayer.score;
        if (!IsGameOver) return;
        TimerText.text = "Going to room in : " + ((int)timerTime - (int)Time.realtimeSinceStartup + (int)timer);

    }

    void FindLocalTank()
    {
        //Check to see if the player is loaded in yet
        if (ClientScene.localPlayer == null)
            return;

        LocalPlayer = ClientScene.localPlayer.GetComponent<TankPlayerController>();
    }

    [Server]
    public void GoBackToRoom()
    {
        NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
        NetworkServer.Destroy(gameObject);
    }
}
