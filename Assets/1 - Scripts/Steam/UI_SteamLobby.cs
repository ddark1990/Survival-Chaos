using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SurvivalChaos
{
    public class UI_SteamLobby : MonoBehaviour
    {
        public static UI_SteamLobby Instance;

        [SerializeField] GameObject lobbyListPanel;
        [SerializeField] GameObject lobbyItemPrefab;
        [SerializeField] RectTransform lobbyListContent;

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
        }

        private void OnDisable()
        {
            NetworkPlayer.OnClientConnected -= HandleOnClientConnected;
            NetworkPlayer.OnClientDisconnected -= HandleOnClientDisconnected;
        }

        private void HandleOnClientDisconnected(NetworkPlayer player)
        {
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

            if (player.hasAuthority)
            {
                OnEnteredLobby?.Invoke();

                //print($"You joined the lobby!");
                PopupManager.instance.ShowPopup($"Joined a lobby!");
            }
            else
            {
                //print($"Player {player.PlayerName} joined the lobby!");
                PopupManager.instance.ShowPopup($"{player.PlayerName} joined!");
            }

        }

        public void OnLobbies_ButtonPress()
        {
            SteamLobby.Instance.GetLobbies();
        }

        internal void PopulateSteamLobbies(List<CSteamID> lobbyIds, LobbyDataUpdate_t result)
        {
            foreach (CSteamID lobbyId in lobbyIds)
            {
                //print($"LobbyName :{SteamMatchmaking.GetLobbyData(lobbyId, "name")} | LobbyId: {lobbyId}");
                //filter out lobbies that have no names nor a word "lobby" in it
                if (SteamMatchmaking.GetLobbyData(lobbyId, "name") == string.Empty) continue;
                if (SteamMatchmaking.GetLobbyData(lobbyId, "my_game") == string.Empty) continue;
                //if (!SteamMatchmaking.GetLobbyData(lobbyId, "name").Contains("lobby")) continue;

                if (lobbyId.m_SteamID == result.m_ulSteamIDLobby)
                {
                    var lobbyItem = Instantiate(lobbyItemPrefab, lobbyListContent.transform);
                    lobbyItem.GetComponent<UI_LobbyItem>().SetLobbyData(lobbyId, result);

                    if (!steamLobbyItems.Contains(lobbyItem.GetComponent<UI_LobbyItem>())) steamLobbyItems.Add(lobbyItem.GetComponent<UI_LobbyItem>());
                }
            }

            //set content to top
            var halfDelta = lobbyListContent.sizeDelta / 2;
            lobbyListContent.localPosition = new Vector3(0, -halfDelta.y, 0);
            lobbyListContent.SetLeft(0);
        }

        internal void ClearAllSteamLobbies()
        {
            if (steamLobbyItems.Count == 0) return;

            foreach (var lobbyItem in steamLobbyItems)
            {
                Destroy(lobbyItem.gameObject);
            }
            steamLobbyItems.Clear();
        }
    }

}