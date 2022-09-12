using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;
using Doozy.Engine.UI;
using System;

namespace SurvivalChaos
{
    public class UI_ReadyCheck : MonoBehaviour
    {
        public static UI_ReadyCheck instance;

        public TextMeshProUGUI[] playerNameTexts;
        [SerializeField] private UIButton readyButton;
        [SerializeField] private TextMeshProUGUI readyButtonText;


        private void OnEnable()
        {
            NetworkPlayerOLD.AuthorityOnPlayerReadyUpdated += UpdateReadyButtonText;
            GameTimer.OnTimeRemaining += OnTimeRemainingUpdateReadyUI;
        }

        private void OnDisable()
        {
            NetworkPlayerOLD.AuthorityOnPlayerReadyUpdated -= UpdateReadyButtonText;
            GameTimer.OnTimeRemaining -= OnTimeRemainingUpdateReadyUI;
        }

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

            InitializePlayerNameInfo();
        }

        public void UpdateReadyText(NetworkPlayerOLD player, bool readyState)
        {
            /*playerNameTexts[player.playerId].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                readyState ? "Ready!" : "Not Ready";*/
        }

        private void UpdateReadyButtonText(bool readyState)
        {
            readyButtonText.text = readyState ? "Unready" : "Ready";
        }

        private void OnTimeRemainingUpdateReadyUI(float timeRemaining)
        {
/*            if(timeRemaining < 3)
                readyButton.Interactable = false;
*/
            if(timeRemaining < 1)
                UIView.HideView("Game", "ReadyCheck");
        }

        private void InitializePlayerNameInfo()
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

        //used by the player ready toggle button once game loads
        //eventually this is where the bonus selection would happen
        public void TogglePlayerReady()
        {
            var networkManager = (GameNetworkManagerOld)NetworkManager.singleton;

            foreach (var player in networkManager.players)
            {
                if (!player.hasAuthority) continue;

                player.CmdSetPlayerReady(!player.playerReady);
            }

            //networkManager.localRoomPlayerRef.CmdSetPlayerReady(!networkManager.localRoomPlayerRef.playerReady);
        }
    }
}
