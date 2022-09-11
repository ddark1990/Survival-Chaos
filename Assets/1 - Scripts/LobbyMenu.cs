using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UITween;

namespace SurvivalChaos
{
    //lobby view should never be disabled when hidden 
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] GameObject[] playerWaitingObjects;
        //[SerializeField] TMP_Text[] playerNameTexts = new TMP_Text[4];
        [SerializeField] Button startGameButton;

        private void OnEnable()
        {
            RoomPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
            RoomPlayer.AuthorityOnPartyOwnerStateUpdated += AuthHandlePartyOwnerStateUpdated;
            RoomPlayer.ClientOnConnected += HandleClientConnected;
            RoomPlayer.ClientOnDisconnected += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            RoomPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
            RoomPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthHandlePartyOwnerStateUpdated;
            RoomPlayer.ClientOnConnected -= HandleClientConnected;
            RoomPlayer.ClientOnDisconnected -= HandleClientDisconnected;
        }

        public void LeaveLobby()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                GameNetworkManagerOld.singleton.StopHost();
                print("Stopping Host...");
            }
            else
            {
                GameNetworkManagerOld.singleton.StopClient();
                print("Stopping Client...");
            }
        }

        void ClientHandleInfoUpdated(RoomPlayer player)
        {
            StartCoroutine(HandleWaitingForPlayerLabelInfo(player));
        }

        IEnumerator HandleWaitingForPlayerLabelInfo(RoomPlayer player)
        {
            yield return new WaitForSeconds(0.1f);
            var players = ((GameNetworkManagerOld)NetworkManager.singleton).players;

            var playerWaitingObject = playerWaitingObjects[player.playerId];
            var labelImage = playerWaitingObject.GetComponent<Image>();
            var origColor = labelImage.color;

            var tempColor = player ? player.playerColor : origColor;

            playerWaitingObject.GetComponentInChildren<TMP_Text>().text = player.playerName;

            LeanTween.value(playerWaitingObject, labelImage.color, tempColor, 1f).setOnUpdate((Color val) => {
                labelImage.color = val;
            });

            for (int i = players.Count; i < playerWaitingObjects.Length; i++)
            {
                playerWaitingObjects[i].GetComponentInChildren<TMP_Text>().text = "Waiting for player...";
            }

            //re enable later
            //startGameButton.interactable = players.Count > 1;
        }

        private void AuthHandlePartyOwnerStateUpdated(bool state)
        {
            startGameButton.gameObject.SetActive(state);
        }

        private void HandleClientConnected()
        {
            //if (!player.hasAuthority) PopupManager.instance.ShowPopup($"{ChatBehaviour.ColorString(player.playerName, player.playerColor)} Connected");
        }

        private void HandleClientDisconnected(RoomPlayer player)
        {
            if(!player.hasAuthority) PopupManager.instance.ShowPopup($"{ChatBehaviour.ColorString(player.playerName, player.playerColor)} Disconnected");
        }

        public void StartGame()
        {
            ((GameNetworkManagerOld)NetworkManager.singleton).localRoomPlayerRef.CmdStartGame();
        }
    }
}