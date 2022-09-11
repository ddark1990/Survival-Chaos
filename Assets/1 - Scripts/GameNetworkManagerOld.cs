using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using Random = UnityEngine.Random;

namespace SurvivalChaos
{
    public class GameNetworkManagerOld : NetworkManager
    {
        [Header("Other")]
        //this is only for the local player, does not know about any other connection, read only from local side
        public RoomPlayer localRoomPlayerRef;

        public bool gameInProgress;

        public List<RoomPlayer> players;
        [Header("Races")]
        public List<Race> races = new List<Race>();
        [Header("Cam")]
        public CameraControl cam;

        [SerializeField] Transform[] startingPositions;

        //public readonly SyncDictionary<int, RoomPlayer> SyncDict_ActivePlayers = new SyncDictionary<int, RoomPlayer>();


        #region Client
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            print("-- Client Connected --");

            CreateNetworkRoomPlayerMessage createRoomPlayerMessage = new CreateNetworkRoomPlayerMessage
            {
                name = NetworkServer.active && NetworkClient.isConnected ? MultiplayerMenu.instance.nameInput.text : JoinMenu.instance.nameInput.text,
                //color = playerColor,
                isHost = NetworkServer.active && NetworkClient.isConnected ? true : false,
                raceId = Random.Range(0, races.Count) //update later to a better way
            };

            conn.Send(createRoomPlayerMessage);
        }

        public override void OnStopClient()
        {
            players.Clear();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            var player = conn.identity.GetComponent<RoomPlayer>();
            print("-- Client Disconnected --");
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            base.OnClientSceneChanged(conn);
            print("OnClientSceneChanged");

            var player = conn.identity.GetComponent<RoomPlayer>();

            //StartCoroutine(player.FUCKASS());
            //player.CmdSpawnPlayerBaseOnServer();

            if (SceneManager.GetActiveScene().name.StartsWith("Game"))
            {
                //SpawnPlayerBase(player);

                /*for (int i = 0; i < players.Count; i++)
                {
                    var startingPlayer = players[i];

                    if (!startingPlayer.hasAuthority) return;

                    startingPlayer.SpawnPlayerBaseOnServer();
                }*/
            }

            player.CmdSetPlayerSceneLoaded(true);
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
/*            if (SceneManager.GetActiveScene().name.StartsWith("MainMenu") && newSceneName.StartsWith("Game"))
            {
                StartCoroutine(UI_LoadingScreen.instance.PresentLoadingScreen(5));
            }
*/            
            StartCoroutine(UI_LoadingScreen.instance.PresentLoadingScreen(2));
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        }        
        #endregion

        #region Server
        public void StartGame()
        {
            gameInProgress = true;

            ServerChangeScene("Game");

            //have rpc here to turn on loading screen before scene starts loading
        }
        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler<CreateNetworkRoomPlayerMessage>(OnCreateNetworkRoomPlayer);
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<CreateNetworkRoomPlayerMessage>();

            players.Clear();

            gameInProgress = false;
        }

/*        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);

            var roomPlayer = conn.identity.GetComponent<RoomPlayer>();

            players.Add(roomPlayer);

            roomPlayer.SetPlayerName(NetworkServer.active && NetworkClient.isConnected ?
                MultiplayerMenu.instance.nameInput.text : JoinMenu.instance.nameInput.text);
            roomPlayer.SetPlayerColor(Random.ColorHSV());
            roomPlayer.SetPlayerHost(NetworkServer.active && NetworkClient.isConnected ? true : false);
        }
*/
        /*public override void OnServerConnect(NetworkConnection conn)
        {
            if (!gameInProgress) return;

            conn.Disconnect();
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            var player = conn.identity.GetComponent<RoomPlayer>();

            players.Remove(player);

            base.OnServerDisconnect(conn);
        }*/

        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);

        }

        //only runs on server
        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            print("OnServerSceneChanged");

            //check when player finished loading scene
            if (SceneManager.GetActiveScene().name.StartsWith("Game"))
            {
                //create players objects at begining of the game
                //base, turrets, barracks
                for (int i = 0; i < players.Count; i++)
                {
                    var player = players[i];

                    SpawnPlayerBase(player);
                }
            }
        }

        private void SpawnPlayerBase(RoomPlayer player)
        {
            //randomize
            var startPosition = startPositions[player.playerId];
            var baseSpawn = startPosition.GetComponent<BaseSpawn>();

            var mainBaseObject = Instantiate(races[player.raceId].mainBase.gameObject, baseSpawn.transform.position, baseSpawn.transform.rotation);
            NetworkServer.Spawn(mainBaseObject, player.connectionToClient);

            var mainBase = mainBaseObject.GetComponent<MainBase>();

            mainBase.owner = player.gameObject;

            mainBase.ServerSetMaterialIndex(player.playerId);

            //set owner id
            mainBase.ServerSetMainBaseOwner(player.playerId);

            AstarPath.active.Scan();
        }

        //roomplayer should create this
        //only called on server, not clients, thats why nothing from client is called from here
        //set basespawntransform over network correctly for all players or change logic based on info up top
        IEnumerator SpawnPlayerBaseOnServer()
        {
            yield return new WaitForSeconds(1);

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];

                var startPosition = startPositions[i];
                var baseSpawn = startPosition.GetComponent<BaseSpawn>();

                var mainBaseObject = Instantiate(races[player.raceId].mainBase, baseSpawn.transform.position, baseSpawn.transform.rotation);
                NetworkServer.Spawn(mainBaseObject.gameObject, player.connectionToClient);

                foreach (var turretSpawn in baseSpawn.turretSpawns)
                {
                    var turretObject = Instantiate(races[player.raceId].turret, turretSpawn.position, turretSpawn.rotation);
                    NetworkServer.Spawn(turretObject.gameObject, player.connectionToClient);
                }

                foreach (var barrackSpawn in baseSpawn.barrackSpawns)
                {
                    var barrackObject = Instantiate(races[player.raceId].barracks, barrackSpawn.position, barrackSpawn.rotation);
                    NetworkServer.Spawn(barrackObject.gameObject, player.connectionToClient);
                    var barracks = barrackObject.GetComponent<Barracks>();

                    //only being set locally, not to other client
                    barracks.waypointInfo = barrackSpawn.GetComponent<WaypointInfo>();
                }

                AstarPath.active.Scan();
            }
        }

        private void OnCreateNetworkRoomPlayer(NetworkConnection conn, CreateNetworkRoomPlayerMessage message)
        {
            GameObject roomPlayerObject = Instantiate(playerPrefab);

            var roomPlayer = roomPlayerObject.GetComponent<RoomPlayer>();

            // call this to use this gameobject as the primary controller
            //NetworkServer.AddPlayerForConnection(conn, roomPlayerObject);

            //applies syncvar
            roomPlayer.ServerSetPlayerName(message.name);

            //roomPlayer.ServerSetPlayerColor(message.color);
            roomPlayer.ServerSetPlayerHost(message.isHost);
            roomPlayer.ServerSetRandomRaceId(message.raceId);
            roomPlayer.ServerSetIncome(25);

        }
        public struct CreateNetworkRoomPlayerMessage : NetworkMessage
        {
            public string name;
            public Color color;
            public bool isHost;
            public int raceId;
        }

        #endregion
    }
}