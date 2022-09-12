using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalChaos 
{
    public class MainBase : Selectable
    {
        //kept static, called only from its own client by UI, etc... with populated objects from the network that you own
        public static event Action<MainBase> OnMainBaseSpawned;

        public UpgradesController upgradeController;

        NetworkPlayerOLD localPlayer;
        List<Race> races;

        private void Awake()
        {
            upgradeController = GetComponent<UpgradesController>();

            CacheGeneralDataComponents();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            //starts the game with mainbase at level 1
            ServerSetUpgradeLevel(1);
        }

        //cant use local player
        [Command]
        private void CreateBuildings()
        {
            //randomize
            var startPosition = NetworkManager.startPositions[0];
            var baseSpawn = startPosition.GetComponent<BaseSpawn>();

            foreach (var turretSpawn in baseSpawn.turretSpawns)
            {
                var turretObject = Instantiate(races[localPlayer.raceId].turret, turretSpawn.position, turretSpawn.rotation);
                NetworkServer.Spawn(turretObject.gameObject, localPlayer.connectionToClient);

                var turret = turretObject.GetComponent<Turret>();

                turret.owner = gameObject;

                turret.ServerSetMaterialIndex(0);

                turret.ServerSetMainBaseOwner(0);

                // player.SyncDict_OwnedBuildings.Add(IdentifierType.Turret, turretObject.gameObject);

            }

            foreach (var barrackSpawn in baseSpawn.barrackSpawns)
            {
                var barrackObject = Instantiate(races[localPlayer.raceId].barracks, barrackSpawn.position, barrackSpawn.rotation);
                NetworkServer.Spawn(barrackObject.gameObject, localPlayer.connectionToClient);

                var barracks = barrackObject.GetComponent<Barracks>();

                barracks.owner = gameObject;

                barracks.ServerSetMaterialIndex(0);

                barracks.ServerSetMainBaseOwner(0);

                //barrackObject.mainBase = mainBaseObject;
                //player.SyncDict_OwnedBuildings.Add(IdentifierType.Barracks, barrackObject.gameObject);

                //only being set locally, not to other client
                barracks.waypointInfo = barrackSpawn.GetComponent<WaypointInfo>();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!hasAuthority) return;

            localPlayer = owner.GetComponent<NetworkPlayerOLD>();
            races = ((GameNetworkManagerOld)NetworkManager.singleton).races;

            CreateBuildings();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            //local client event
            OnMainBaseSpawned?.Invoke(this);
        }
    }
}