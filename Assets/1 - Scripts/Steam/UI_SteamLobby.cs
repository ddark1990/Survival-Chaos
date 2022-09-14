using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class UI_SteamLobby : MonoBehaviour
    {
        public static UI_SteamLobby Instance;

        public Color[] PlayerColors;
        [SerializeField] UI_LobbyPlayerItem[] lobbyPlayerItems;

        [SerializeField] GameObject lobbyListPanel;
        [SerializeField] GameObject lobbyItemPrefab;
        [SerializeField] RectTransform lobbyListContent;
        [SerializeField] Button startGameButton;
        [SerializeField] Button leaveButton;
        [SerializeField] TextMeshProUGUI startTimerText;

        List<UI_LobbyItem> steamLobbyItems = new List<UI_LobbyItem>();

        public UnityEvent OnEnteredLobby;
        public UnityEvent OnLeftLobby;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            NetworkPlayer.OnClientConnected += HandleOnClientConnected;
            NetworkPlayer.OnClientDisconnected += HandleOnClientDisconnected;
            NetworkPlayer.OnHostClosedConnection += HandleOnHostClosedConnection;

            LobbyGameStarter.OnGameStartedTimerCountdown += HandleGameStartedTimer;
        }

        private void OnDisable()
        {
            NetworkPlayer.OnClientConnected -= HandleOnClientConnected;
            NetworkPlayer.OnClientDisconnected -= HandleOnClientDisconnected;
            NetworkPlayer.OnHostClosedConnection -= HandleOnHostClosedConnection;

            LobbyGameStarter.OnGameStartedTimerCountdown -= HandleGameStartedTimer;
        }

        private void HandleGameStartedTimer(int timer)
        {
            UpdateStartTimerText(timer);
        }

        //doesnt work
        private void HandleOnHostClosedConnection(NetworkPlayer player)
        {
            print("HandleOnHostClosedConnection");
        }
        
        private void HandleOnClientDisconnected(NetworkPlayer player)
        {
            foreach (var item in lobbyPlayerItems) //finds first available lobby item
            {
                if (!item.IsOccupied || item.Player != player) continue;

                item.ResetLobbyPlayerItem();

                break;
            }

            if (player.hasAuthority)
            {
                OnLeftLobby?.Invoke();

                //print($"You joined the lobby!");
                //PopupManager.instance.ShowPopup($"");
            }
            else
            {
                //print($"Player {player.PlayerName} joined the lobby!");
                PopupManager.instance.ShowPopup($"{player.PlayerName} left!");
            }
        }

        private void HandleOnClientConnected(NetworkPlayer player)
        {
            StartCoroutine(HandlePlayerJoinLobby(player));
        }

        private IEnumerator HandlePlayerJoinLobby(NetworkPlayer player)
        {
            yield return new WaitUntil(() => player.netIdentity.GetComponent<NetworkPlayer>().SteamId != 0);
            
            foreach (var item in lobbyPlayerItems) //finds first available lobby item
            {
                if (item.IsOccupied) continue;

                item.InitializeLobbyPlayerItem(player);

                break;
            }

            if (player.hasAuthority)
            {
                OnEnteredLobby?.Invoke();

                if (player.IsHost) EnableHostOptions();

                PopupManager.instance.ShowPopup($"Joined a lobby!");
            }
            else
            {
                PopupManager.instance.ShowPopup($"{player.PlayerName} joined!");
            }

            //startGameButton.interactable = NetworkServer.connections.Count > 1;
        }

        public void UpdateStartTimerText(int timer)
        {
            if (!startTimerText.gameObject.activeInHierarchy) startTimerText.gameObject.SetActive(true);

            if (timer < 3) leaveButton.interactable = false;

            if (timer > -1) startTimerText.text = timer.ToString();

            if (!NetworkClient.active) startTimerText.gameObject.SetActive(false); //TODO: disable when not counting down
        }

        private void EnableHostOptions()
        {
            startGameButton.gameObject.SetActive(true);
        }

        public void OnButtonPress_StartGame()
        {
            LobbyGameStarter.Instance.Cmd_StartGame();
        }

        public void OnButtonPress_GetLobbies()
        {
            SteamLobby.Instance.GetLobbies();
        }

        public void OnButtonPress_LeaveLobby()
        {
            if (NetworkServer.active && NetworkClient.isConnected) //host
            {
                GameNetworkManager.singleton.StopHost();
                print("Stopping Host...");
            }
            else
            {
                GameNetworkManager.singleton.StopClient();
                
                print("Stopping Client...");
            }
        }

        internal void PopulateSteamLobbies(List<CSteamID> lobbyIds)
        {
            foreach (CSteamID lobbyId in lobbyIds)
            {
                //filter out lobbies that have no tag "my_game"
                if (SteamMatchmaking.GetLobbyData(lobbyId, "my_game") == string.Empty) continue;

                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyListContent.transform);
                lobbyItem.GetComponent<UI_LobbyItem>().InitializeLobbyItem(lobbyId);

                if (!steamLobbyItems.Contains(lobbyItem.GetComponent<UI_LobbyItem>())) steamLobbyItems.Add(lobbyItem.GetComponent<UI_LobbyItem>());
            }

            //set content to top
            var halfDelta = lobbyListContent.sizeDelta / 2;
            lobbyListContent.localPosition = new Vector3(0, -halfDelta.y, 0);
            lobbyListContent.SetLeft(0);

            //print("Populating Steam Lobbies..");
        }

        public void ClearAllSteamLobbies()
        {
            //print("Clearing Steam Lobbies...");
            if (steamLobbyItems.Count == 0) return;

            foreach (var lobbyItem in steamLobbyItems)
            {
                Destroy(lobbyItem.gameObject);
            }
            steamLobbyItems.Clear();
        }
    }

}