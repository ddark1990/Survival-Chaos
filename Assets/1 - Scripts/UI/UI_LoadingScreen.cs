using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Doozy.Engine.UI;

namespace SurvivalChaos
{
    public class UI_LoadingScreen : MonoBehaviour
    {
        public static UI_LoadingScreen instance;
        public TextMeshProUGUI[] playerNameTexts;

        private void Awake()
        {
            // if instance is not yet set, set it and make it persistent between scenes
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // if instance is already set and this is not the same object, destroy it
                if (this != instance) { Destroy(gameObject); }
            }
        }

        /*public void SetPlayerReadyInfo()
        {
            var networkManager = ((GameNetworkManager)NetworkManager.singleton);

            for (int i = 0; i < networkManager.players.Count; i++)
            {
                print(networkManager.players.Count);

                var player = networkManager.players[i];
                //seperate 
                //if (player != networkManager.localRoomPlayerRef) continue;
                //fix, make activate 2 names and set one 
                if(!playerNameTexts[i].gameObject.activeInHierarchy) playerNameTexts[i].gameObject.SetActive(true);
                playerNameTexts[i].text = $"{player.playerName}: Ready - {player.finishedLoading}";

            }
        }*/

        public IEnumerator PresentLoadingScreen(float timer)
        {
            SetPlayerNameInfo();
            UIView.ShowView("General", "Loading");

            yield return new WaitForSeconds(timer);

            UIView.HideView("General", "Loading");
            UIView.ShowView("Game", "ReadyCheck");
        }

        public void SetPlayerNameInfo()
        {
            var networkManager = (GameNetworkManagerOld)NetworkManager.singleton;

            for (int i = 0; i < networkManager.players.Count; i++)
            {
                var player = networkManager.players[i];

                playerNameTexts[i].gameObject.SetActive(true);
                playerNameTexts[i].text = $"{player.playerName}";
                playerNameTexts[i].color = player.playerColor;
            }
        }

        public void SetPlayerReadyInfo(RoomPlayer player, int playerIncrement)
        {
            playerNameTexts[playerIncrement].text = $"{player.playerName}: Ready: {player.finishedLoading}";
        }
    }
}