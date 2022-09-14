using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SyncVar]
        private ulong steamId;
        public ulong SteamId => steamId;

        [SyncVar(hook = nameof(HandleDisplayNameUpdated))]
        private string playerName;
        public string PlayerName => playerName;

        [SyncVar(hook = nameof(HandlePartyOwnerStateUpdated))]
        private bool isHost;
        public bool IsHost => isHost;

        [SyncVar(hook = nameof(HandlePlayerReadyUpdated))]
        private bool playerReady;
        public bool PlayerReady => playerReady;

        [SyncVar]
        private Color playerColor;
        public Color PlayerColor => playerColor;

        [SyncVar(hook = nameof(HandleRaceIdStateUpdated))]
        private int raceId;
        public int RaceId => raceId;

        [SyncVar(hook = nameof(HandleLoadingSceneStateUpdated))]
        private bool isFinishedLoading;
        public bool IsFinishedLoadingScene => isFinishedLoading;

        public static event Action<NetworkPlayer> OnClientConnected;
        public static event Action<NetworkPlayer> OnClientDisconnected;

        public static event Action<NetworkPlayer> OnClientInfoUpdated;

        public static event Action<bool> OnHostStateUpdated;
        public static event Action<bool> OnPlayerReadyUpdated;
        public static event Action<int> OnRaceIdStateUpdated;
        public static event Action<bool> OnLoadingSceneStateUpdated;
        public static event Action<NetworkPlayer> OnHostClosedConnection;

        #region Server

        [Server]
        public void ServerSetSteamId(ulong newId)
        {
            steamId = newId;
        }

        [Server]
        public void ServerSetPlayerName(string newName)
        {
            playerName = newName;
        }

        [Server]
        public void ServerSetPlayerHost(bool isHost)
        {
            this.isHost = isHost;
        }

        [Server]
        public void ServerSetPlayerColor(Color newColor)
        {
            playerColor = newColor;
        }

        #endregion

        #region Client

        public override void OnStartClient()
        {
            base.OnStartClient();

            OnClientConnected?.Invoke(this);

            DontDestroyOnLoad(gameObject);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            OnClientDisconnected?.Invoke(this);

            if (!NetworkServer.active) NetworkClient.OnTransportDisconnected();

            if (!hasAuthority) return;

            var lobbyId = new CSteamID(SteamLobby.CurrentLobbyID);

            SteamMatchmaking.LeaveLobby(lobbyId);
        }

        [ClientRpc]
        public void RPC_OnHostConnectionClosedEvent()
        {
            OnHostClosedConnection?.Invoke(this);
        }
        #endregion

        #region SyncVar Hooks
        private void HandlePartyOwnerStateUpdated(bool oldState, bool newState)
        {
            if (!hasAuthority) return;

            OnHostStateUpdated?.Invoke(newState);
        }
        private void HandleLoadingSceneStateUpdated(bool oldState, bool newState)
        {
            if (!hasAuthority) return;

            OnLoadingSceneStateUpdated?.Invoke(newState);
        }
        private void HandleRaceIdStateUpdated(int oldId, int newId)
        {
            if (!hasAuthority) return;

            OnRaceIdStateUpdated?.Invoke(newId);
        }
        private void HandlePlayerReadyUpdated(bool oldPlayerReady, bool newPlayerReady)
        {
            if (!hasAuthority) return;

            OnPlayerReadyUpdated?.Invoke(newPlayerReady);
        }
        private void HandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
        {
            OnClientInfoUpdated?.Invoke(this);
        }
        #endregion


    }
}
