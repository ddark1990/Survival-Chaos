using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace SurvivalChaos
{
    public class Barracks : Selectable
    {
        [SerializeField] Transform spawningPoint;
        //move out
        [SerializeField] TextMeshProUGUI spawnTimerText;

        //client only
        //mainbase owner
        //fix
        [HideInInspector] public WaypointInfo waypointInfo;

        [SyncVar]
        float spawnRate = 10;
        [SyncVar]
        double spawnTimer;

        //local event
        public static event Action<Transform> OnBarracksSpawned;

        private void Awake()
        {
            CacheGeneralDataComponents();

        }

        private void Start()
        {
            //if(hasAuthority) mainBase = GetMainBase();
            //print(mainBase.upgradeController.syncDict_ActiveUpgradeData.Count);

            ServerSetSpawnRate(38);
            ServerSetSpawnTimer(spawnRate);

            if (!hasAuthority) spawnTimerText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (GameTimer.instance.countdownTimeRemaining > 0) return;

            if (isServer)
            {
                spawnTimer -= Time.deltaTime;

                if (spawnTimer > 0) return;

                //ServerSpawnUnits(unitTierList);
            }

            if(hasAuthority)
            {
                spawnTimerText.text = spawnTimer.ToString("#");
            }
            
        }

        #region Server

        [Server]
        private void ServerSpawnUnits(List<Unit> units)
        {
            var points = PoissonDiscSampling.GeneratePoints(1, Vector2.one * 3, 30);

            for (int i = 0; i < (scriptableObjectData as BarracksScriptableData).units.Length; i++)
            {
                var unit = (scriptableObjectData as BarracksScriptableData).units[i];

                //if (unit.scriptableObjectData.sendType != )

                var localUnitObject = Instantiate(unit.gameObject, spawningPoint.position + new Vector3(points[i].x, 0, points[i].y), spawningPoint.rotation);
                NetworkServer.Spawn(localUnitObject, connectionToClient);

                var localUnit = localUnitObject.GetComponent<Unit>();


                ///client logic
                ///
                //localUnit.RpcSetUnitOwningBarracks(gameObject);

                //sets ai initial destination to the first waypoint
                //localUnit.GetUnitMovement().ServerSetUnitDestination(waypointInfo.halfWayPoint.position);
                ///dont work
                ///
                //localUnit.GetTargeter().RpcSetTarget(waypointInfo.halfWayPoint.gameObject);

                ///server logic
                ///
                //sets the material based on the playerID which corolates to unitColorMaterials order
                localUnit.ServerSetMaterialIndex(materialIndex);

            }

            spawnTimer = spawnRate;
        }
        
        public void TrySendUnit(IdentifierType identifierType)
        {
            
            CmdSendUnit(identifierType);

        }

        [Command]
        private void CmdSendUnit2(GameObject unitGameObject, int index)
        {
            var points = PoissonDiscSampling.GeneratePoints(1, Vector2.one * 3, 30);

            var localUnitObject = Instantiate(unitGameObject.gameObject, spawningPoint.position + new Vector3(points[index].x, 0, points[index].y), spawningPoint.rotation);
            NetworkServer.Spawn(localUnitObject, connectionToClient);

            var localUnit = localUnitObject.GetComponent<Unit>();
            var syncDict_ActiveStatData = localUnit.SyncDict_ActiveStatData;
            //var syncDict_ActiveUpgradeData = mainBase.upgradeController.SyncDict_ActiveUpgradeData;

            ///client logic
            ///
            //localUnit.RpcSetUnitOwningBarracks(gameObject);

            //sets ai initial destination to the first waypoint
            localUnit.GetUnitMovement().ServerSetUnitDestination(waypointInfo.barracksWaypoints[0].position);

            ///server logic
            ///
            //sets the material based on the playerID which corolates to unitColorMaterials order
            localUnit.ServerSetMaterialIndex(materialIndex);
        }

        [Command]
        public void CmdSendUnit(IdentifierType identifierType)
        {
            var points = PoissonDiscSampling.GeneratePoints(1, Vector2.one * 3, 30);

            for (int i = 0; i < (scriptableObjectData as BarracksScriptableData).units.Length; i++)
            {
                var unit = (scriptableObjectData as BarracksScriptableData).units[i];

                if (unit.scriptableObjectData.identifierType != identifierType) continue;

                var localUnitObject = Instantiate(unit.gameObject, spawningPoint.position + new Vector3(points[i].x, 0, points[i].y), spawningPoint.rotation);
                NetworkServer.Spawn(localUnitObject, connectionToClient);

                var localUnit = localUnitObject.GetComponent<Unit>();
                localUnit.owner = gameObject;
                //TODO: get upgrade info somehow from mainbase or move upgrade info elsewhere
                var syncDict_ActiveStatData = localUnit.SyncDict_ActiveStatData;
                var syncDict_ActiveUpgradeData = owner.GetComponent<MainBase>().upgradeController.SyncDict_ActiveUpgradeData;

                //TODO: need to get mainbase reference somehow so i can pull upgrade data & add to unit on spawn
                //modify attack based on upgrades

                /*                var ownerPlayerId = GetownerPlayerId();

                                var upgradeDataSets = mainBase.upgradeController.syncDict_ActiveUpgradeData;
                                var statDataSet = upgradeDataSets[identifierType];
                */
                for (int z = 0; z < localUnit.scriptableObjectData.statData.Length; z++)
                {
                    var data = localUnit.scriptableObjectData.statData[z];

                    //var baseStat = statDataSet[z];
                    //var stat = syncList_StatData[z];

                    //set stats on start, and make sure were adding the upgraded list to the base stats
                    //check if order matters
                    foreach (var item in syncDict_ActiveStatData)
                    {
                        var key = item.Key;
                        var value = item.Value;

                        if (data.type == key)
                        {
                            value = data.value;
                        }
                    }
                }

                //localUnit.ServerSetAttack();


                ///client logic
                ///
                //localUnit.RpcSetUnitOwningBarracks(gameObject);

                //sets ai initial destination to the first waypoint
                localUnit.GetUnitMovement().ServerSetUnitDestination(waypointInfo.barracksWaypoints[0].position);

                ///server logic
                ///
                //sets the material based on the playerID which corolates to unitColorMaterials order
                localUnit.ServerSetMaterialIndex(materialIndex);
            }
        }
        
        [Command]
        public void CmdRepairBarracksOverTime()
        {
            
        }

        [Server]
        public void ServerSetSpawnRate(int spawnRate)
        {
            this.spawnRate = spawnRate;
        }

        [Server]
        public void ServerSetSpawnTimer(float spawnTimer)
        {
            this.spawnTimer = spawnTimer;
        }

        [Command]
        public void CmdSetOwner(GameObject owner)
        {
            RpcSetOwner(owner);
        }
        [ClientRpc]
        public void RpcSetOwner(GameObject owner)
        {
            //this.owner = owner;
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!hasAuthority) return;

            OnBarracksSpawned?.Invoke(transform);

            /*var mainBases = FindObjectsOfType<MainBase>();

            foreach (var mainBase in mainBases)
            {
                if (mainBase.hasAuthority)
                    owner = mainBase;
            }*/
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
        }
        #endregion

        #region Client


        #endregion
    }
}
