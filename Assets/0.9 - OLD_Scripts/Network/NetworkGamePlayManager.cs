using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class NetworkGamePlayManager : NetworkBehaviour
    {
        /// <summary>
        /// default interval is 25 - 30 seconds
        /// </summary>
        int goldIncomeGiveInterval = 25;
        float goldIncomeTimer;

        GameNetworkManagerOld networkManager;

        private void Start()
        {
            networkManager = (GameNetworkManagerOld)NetworkManager.singleton;
        }

        [ServerCallback]
        private void Update()
        {
            if (!isServer) return;
            //debug
            /*if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (var player in networkManager.players)
                {
                    player.ServerAddGold(player.GetPlayerIncome());
                }
            }
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                foreach (var player in networkManager.players)
                {
                    player.ServerAddIncome(25);
                }
            }*/

            //give gold income
            goldIncomeTimer += Time.deltaTime;

            if (goldIncomeTimer > goldIncomeGiveInterval)
            {
                ServerGiveIncomeGoldToAllPlayers();

                goldIncomeTimer = 0;
            }
        }

        [Server]
        private void ServerGiveIncomeGoldToAllPlayers()
        {
            foreach (var player in networkManager.players)
            {
                player.ServerAddGold(player.GetPlayerIncome());
            }
        }
    }
}
