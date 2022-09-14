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

        [Header("Ref")]
        public Color[] PlayerColors;
        [SerializeField] UI_LobbyPlayerItem[] lobbyPlayerItems;
        [SerializeField] GameObject lobbyListPanel;
        [SerializeField] GameObject lobbyItemPrefab;
        [SerializeField] RectTransform lobbyListContent;
        [SerializeField] Button startGameButton;
        [SerializeField] Button cancelStartGameButton;
        [SerializeField] Button leaveButton;
        [SerializeField] TextMeshProUGUI startTimerText;

        [Header("Lobby Events")]
        public UnityEvent OnEnteredLobby;
        public UnityEvent OnLeftLobby;

        [Header("Game Started Timer Events")]
        public UnityEvent<string> OnGameStartedTimerStarted;
        public UnityEvent<string> OnGameStartedTimerUpdated;
        public UnityEvent OnGameStarted;
        public UnityEvent<bool> OnGameStartedCancel;

        List<UI_LobbyItem> steamLobbyItems = new List<UI_LobbyItem>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            NetworkPlayer.OnClientConnected += HandleOnClientConnected;
            NetworkPlayer.OnClientDisconnected += HandleOnClientDisconnected;

            LobbyGameStarter.OnGameStartedTimerCountdownStarted += HandleGameStartedTimerStarted;
            LobbyGameStarter.OnGameStartedTimerCountdown += HandleGameStartedTimerUpdated;
            LobbyGameStarter.OnGameStartedCancel += HandleGameStartedCancel;
            LobbyGameStarter.OnGameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            NetworkPlayer.OnClientConnected -= HandleOnClientConnected;
            NetworkPlayer.OnClientDisconnected -= HandleOnClientDisconnected;

            LobbyGameStarter.OnGameStartedTimerCountdownStarted -= HandleGameStartedTimerStarted;
            LobbyGameStarter.OnGameStartedTimerCountdown -= HandleGameStartedTimerUpdated;
            LobbyGameStarter.OnGameStartedCancel -= HandleGameStartedCancel;
            LobbyGameStarter.OnGameStarted -= HandleGameStarted;
        }

        private void HandleOnClientConnected(NetworkPlayer player)
        {
            StartCoroutine(HandlePlayerJoinLobby(player));
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

        private void HandleGameStartedTimerStarted(int startedTime)
        {
            OnGameStartedTimerStarted?.Invoke(startedTime.ToString());
        }

        private void HandleGameStartedTimerUpdated(int timer)
        {
            if (timer < 1) return;

            OnGameStartedTimerUpdated?.Invoke(timer.ToString());

            UI_UpdateStartTimerText(timer);
        }

        private void HandleGameStartedCancel(bool state)
        {
            if (state) OnGameStartedCancel?.Invoke(state);
            //print($"HandleGameStartedCancel | {state}");
        }

        private void HandleGameStarted()
        {
            OnGameStarted?.Invoke();
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

                if (player.IsHost) ToggleHostOptions(true);

                PopupManager.instance.ShowPopup($"Joined a lobby!");
            }
            else
            {
                PopupManager.instance.ShowPopup($"{player.PlayerName} joined!");
            }

            //startGameButton.interactable = NetworkServer.connections.Count > 1;
        }

        public void UI_UpdateStartTimerText(int timer)
        {
            if (timer < 3 && !LobbyGameStarter.Instance.GameStartedCanceled)
            {
                cancelStartGameButton.interactable = false;
                leaveButton.interactable = false;
            }
        }

        private void ToggleHostOptions(bool toggle)
        {
            ResetButtons();

            startGameButton.gameObject.SetActive(toggle);
            cancelStartGameButton.gameObject.SetActive(toggle);
        }

        private void ResetButtons()
        {
            cancelStartGameButton.interactable = true;
            leaveButton.interactable = true;
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

        public void OnButtonPress_StartGame()
        {
            if (!GameNetworkManager.NetworkPlayer.IsHost) return;

            LobbyGameStarter.Instance.Cmd_StartGame();
        }

        public void OnButtonPress_GetLobbies()
        {
            SteamLobby.Instance.GetLobbies();
        }

        public void OnButtonPress_CancelGameStart()
        {
            if (!GameNetworkManager.NetworkPlayer.IsHost) return;

            LobbyGameStarter.Instance.Cmd_CancelStartGame();
        }

        public void OnButtonPress_LeaveLobby()
        {
            if (NetworkServer.active && NetworkClient.isConnected) //host
            {
                if (LobbyGameStarter.Instance.StartGameTimer > 0) LobbyGameStarter.Instance.Cmd_CancelStartGame();

                GameNetworkManager.singleton.StopHost();
                print("Stopping Host...");
            }
            else
            {
                GameNetworkManager.singleton.StopClient();

                print("Stopping Client...");
            }
        }
    }

}