using UnityEngine;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SurvivalChaos
{
    public class NetworkPlayerOLD : NetworkBehaviour
    {
        [SyncVar]
        public ulong steamId;
        //make getters 
        [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
        public string playerName;
        [SyncVar]
        public Color playerColor;
        [SyncVar(hook = nameof(AuthorityHandlePlayerReadyUpdated))]
        public bool playerReady;
        [SyncVar(hook = nameof(AuthorityHandleRaceIdStateUpdated))]
        public int raceId;
        [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
        public bool isHost;
        [SyncVar(hook = nameof(AuthorityHandleLoadingSceneStateUpdated))]
        public bool finishedLoading;

        public readonly SyncDictionary<int, GameObject> SyncDict_OwnedBuildings = new SyncDictionary<int, GameObject>();
        //null on client for some reason, try to just get locally, once scene changes or event fires off for minabase created
        public readonly SyncList<GameObject> SyncList_OwnedBuildings = new SyncList<GameObject>();
        public List<GameObject> ownedBuildings = new List<GameObject>();

        [Server]
        public void ServerAddOwnedBuilding(int typeId, GameObject building)
        {
            SyncDict_OwnedBuildings.Add(typeId, building);
        }
        [Command]
        public void CmdAddOwnedBuilding(GameObject building)
        {
            ServerAddOwnedBuilding(building);
        }
        [Server]
        public void ServerAddOwnedBuilding(GameObject building)
        {
            ownedBuildings.Add(building);
            RpcAddOwnedBuilding(building);
        }
        [ClientRpc]
        public void RpcAddOwnedBuilding(GameObject building)
        {
            ownedBuildings.Add(building);
        }

        //gameplay
        [SyncVar(hook = nameof(AuthorityHandlePlayerGoldUpdated))]
        private double playerGold;
        public double GetPlayerGold() => playerGold;

        [SyncVar/*(hook = nameof(AuthorityHandlePlayerGoldUpdated))*/]
        private double playerIncome;
        public double GetPlayerIncome() => playerIncome;

        [SyncVar(hook = nameof(AuthorityHandlePlayerScoreUpdated))]
        private double playerScore;
        public double GetPlayerScore() => playerScore;
        //

        GameNetworkManagerOld gameNetworkManager;

        public static event Action ClientOnConnected;
        public static event Action<NetworkPlayerOLD> ClientOnDisconnected;

        public static event Action<NetworkPlayerOLD> ClientOnInfoUpdated;

        public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
        public static event Action<bool> AuthorityOnLoadingSceneStateUpdated;
        public static event Action<int> AuthorityOnRaceIdStateUpdated;
        public static event Action<bool> AuthorityOnPlayerReadyUpdated;
        public static event Action<double> AuthorityOnPlayerGoldUpdated;
        public static event Action<double> AuthorityOnPlayerScoreUpdated;


        #region Unity
        private void Awake()
        {
/*            gameNetworkManager = ((GameNetworkManagerOld)NetworkManager.singleton);

            ((GameNetworkManagerOld)NetworkManager.singleton).players.Add(this);

            playerId = ((GameNetworkManagerOld)NetworkManager.singleton).players.Count - 1;
*/        }

        #endregion

        #region Server

        public override void OnStartServer()
        {
            base.OnStartServer();
            DontDestroyOnLoad(gameObject);

            var colorArray = new Color[] { Color.green, Color.red, Color.blue, /*purple*/new Color(118, 10, 85) };
            var playerColor = colorArray[1];

            ServerSetPlayerColor(playerColor);

        }

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
        public void ServerSetPlayerColor(Color newColor)
        {
            playerColor = newColor;
        }

        [Server]
        public void ServerSetPlayerHost(bool isHost)
        {
            this.isHost = isHost;
        }

        [Server]
        public void ServerSetRandomRaceId(int newRaceId)
        {
            raceId = newRaceId;
        }

        [Command]
        public void CmdSetPlayerReady(bool readyState)
        {
            ServerSetPlayerReady(readyState);
            RpcSetReadyText(readyState);

            GameTimer.instance.ServerUpdatePlayerReadyIncrement(readyState);
        }
        [Server]
        public void ServerSetPlayerReady(bool readyState)
        {
            playerReady = readyState;
        }

        [Command]
        public void CmdSetPlayerSceneLoaded(bool finishedLoading)
        {
            this.finishedLoading = finishedLoading;
        }

        [Command]
        public void CmdStartGame()
        {
            if (!isHost) return;

            ((GameNetworkManagerOld)NetworkManager.singleton).StartGame();
        }

        #endregion

        [Command]
        public void CmdSpawnPlayerBaseOnServer()
        {
            var startPosition = NetworkManager.startPositions[0];
            var baseSpawn = startPosition.GetComponent<BaseSpawn>();

            var mainBaseObject = Instantiate(gameNetworkManager.races[raceId].mainBase, baseSpawn.transform.position, baseSpawn.transform.rotation);

            NetworkServer.Spawn(mainBaseObject.gameObject, connectionToClient);

            //mainBaseObject.ServerSetMaterialIndex(playerId);

            foreach (var turretSpawn in baseSpawn.turretSpawns)
            {
                var turretObject = Instantiate(gameNetworkManager.races[raceId].turret, turretSpawn.position, turretSpawn.rotation);
                NetworkServer.Spawn(turretObject.gameObject, connectionToClient);

                //turretObject.ServerSetMaterialIndex(playerId);
            }

            foreach (var barrackSpawn in baseSpawn.barrackSpawns)
            {
                var barrackObject = Instantiate(gameNetworkManager.races[raceId].barracks, barrackSpawn.position, barrackSpawn.rotation);
                NetworkServer.Spawn(barrackObject.gameObject, connectionToClient);

                //barrackObject.ServerSetMaterialIndex(playerId);

                //only being set locally, not to other client
                //barrackObject.waypointInfo = barrackSpawn.GetComponent<WaypointInfo>();

                //barrackObject.RpcSetOwner(gameObject);
            }

            AstarPath.active.Scan();
        }

        [Server]
        public void ServerAddGold(double amount)
        {
            playerGold += amount;
        }
        [Server]
        public void ServerAddScore(double amount)
        {
            playerScore += amount;
        }
        [Server]
        public void ServerAddIncome(double amount)
        {
            playerIncome += amount;
        }
        [Server]
        public void ServerSetIncome(double amount)
        {
            playerIncome = amount;
        }

        #region Client

        public override void OnStartClient()
        {
            base.OnStartClient();

            DontDestroyOnLoad(gameObject);

            ClientOnConnected?.Invoke();
        }

        public override void OnStopClient()
        {
            ClientOnInfoUpdated?.Invoke(this);
            ClientOnDisconnected?.Invoke(this);

            if (!isClientOnly) return;

            ((GameNetworkManagerOld)NetworkManager.singleton).players.Remove(this);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            SetNetworkPlayerReference();
        }
        #region RPC

        [ClientRpc]
        public void RpcSetReadyText(bool readyState)
        {
            UI_ReadyCheck.instance.UpdateReadyText(this, readyState);
        }

        #endregion

        #region SyncVar Hooks
        private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
        {
            if (!hasAuthority) return;

            AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);

            //move and hook to above event
            PopupManager.instance.ShowPopup("Hosting Successful");
        }
        private void AuthorityHandleLoadingSceneStateUpdated(bool oldState, bool newState)
        {
            //CmdSpawnPlayerBaseOnServer();

            if (!hasAuthority) return;

            AuthorityOnLoadingSceneStateUpdated?.Invoke(newState);
        }
        private void AuthorityHandleRaceIdStateUpdated(int oldId, int newId)
        {
            if (!hasAuthority) return;

            AuthorityOnRaceIdStateUpdated?.Invoke(newId);
        }
        private void AuthorityHandlePlayerReadyUpdated(bool oldPlayerReady, bool newPlayerReady)
        {
            if (!hasAuthority) return;

            AuthorityOnPlayerReadyUpdated?.Invoke(newPlayerReady);
        }
        private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
        {
            ClientOnInfoUpdated?.Invoke(this);
        }
        private void AuthorityHandlePlayerGoldUpdated(double oldGold, double newGold)
        {
            if (!hasAuthority) return;

            AuthorityOnPlayerGoldUpdated?.Invoke(newGold);
        }
        private void AuthorityHandlePlayerScoreUpdated(double oldScore, double newScore)
        {
            if (!hasAuthority) return;

            AuthorityOnPlayerScoreUpdated?.Invoke(newScore);
        }
        #endregion

        private void SetNetworkPlayerReference()
        {
            ((GameNetworkManagerOld)NetworkManager.singleton).localRoomPlayerRef = this;
        }

        #endregion

    }
}