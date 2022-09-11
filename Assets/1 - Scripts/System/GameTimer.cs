using Mirror;
using SurvivalChaos;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fenki started this badboy
/// </summary>
public class GameTimer : NetworkBehaviour
{
    public static GameTimer instance;

    [SerializeField] TextMeshProUGUI textTimer;

    [SyncVar]
    public float countdownTimeRemaining = 6;
    [SyncVar] 
    private float timeOfTheGame = 0;
    [SyncVar] 
    public bool beginTimer;
    [SyncVar]
    int readyIncrement;

    GameNetworkManagerOld networkManager;

    public static event Action<float> OnTimeRemaining;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // if instance is already set and this is not the same object, destroy it
            if (this != instance) { Destroy(gameObject); }
        }

        networkManager = (GameNetworkManagerOld)NetworkManager.singleton;
    }
    void Update()
    {
        if(countdownTimeRemaining > 0) OnTimeRemaining?.Invoke(countdownTimeRemaining);

        if (isServer)
        {
            if (beginTimer == false) return;

            if (countdownTimeRemaining > 0)
            {
                countdownTimeRemaining -= Time.deltaTime;
            }
            else
            {
                timeOfTheGame += Time.deltaTime;                
            }
        }

        if (countdownTimeRemaining > 0)
        {
            DisplayTime(countdownTimeRemaining);
        }
        else
        {
            DisplayTime(timeOfTheGame);
        }
    }

    [Server]
    public void ServerUpdatePlayerReadyIncrement(bool readyState)
    {
        if (readyState == true) 
            readyIncrement++;
        else 
            readyIncrement--;

        if(readyIncrement == networkManager.players.Count)
        {
            //game ready to start
            beginTimer = true;
        }
        else
        {
            beginTimer = false;
            ResetTimers();
        }
    }

    private void ResetTimers()
    {
        countdownTimeRemaining = 6;
        timeOfTheGame = 0;
    }

    void DisplayTime(float timeToDisplay)
    {
        if (!beginTimer)
        {
            textTimer.text = "0:00";
            return;
        }

        if (timeToDisplay <= 1)
        {
            textTimer.text = "Start!";
            return;
        }

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

/*    void DisplayTime(double timeToDisplay)
    {
        textTimer.text = string.Format("{0}", timeToDisplay);
    }
*/}
