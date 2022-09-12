using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurvivalChaos.UpgradeScriptableData;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;

//MUST INCORPORATE TURRET IN THE LOGIC, OR **MAKE GENERAL ENOUGH TO WORK WITH ANY OBJECT
namespace SurvivalChaos
{
    public class ActiveUpgradeData
    {
        public int orderId;
        public int upgradeLevel;
        public StatUpgrade statUpgrade;
        public StatUpgrade nextStatUpgrade;

        public ActiveUpgradeData(int orderId, int upgradeLevel, StatUpgrade statUpgrade, StatUpgrade nextStatUpgrade)
        {
            this.orderId = orderId;
            this.upgradeLevel = upgradeLevel;
            this.statUpgrade = statUpgrade;
            this.nextStatUpgrade = nextStatUpgrade;
        }
    }

    public class UpgradesController : NetworkBehaviour
    {
        public UpgradeScriptableData[] upgradesData;

        //dont need
        //upgrade events
        public static event Action<UI_UpgradeButton> OnUpgrade;
        public static event Action<UI_UpgradeButton> NextUpgradeFailed;
        public static event Action<UI_UpgradeButton> OnReachedMaxUpgrades;

        //finish server methods
        [SyncVar(hook = nameof(OnUpgradeTimerUpdated))]
        private int upgradeTimer;
        public int GetUpgradeTimer() => upgradeTimer;

        [SyncVar(hook = nameof(OnUpgradeTimerStarted))]
        private bool upgradeTimerStarted;
        public bool GetUpgradeTimerStarted() => upgradeTimerStarted;

        public event Action<int> Event_UpgradeTimerUpdated;
        public event Action<bool, ActiveUpgradeData> Event_UpgradeTimerStarted;
        public event Action<ActiveUpgradeData> Event_OnUpgrade;

        //upgrade level of the object its going to use for checks on upgrade prerequisiteData
        //mainBase is the only object being used to check upgrade level in this game,
        //but im making it generic enough for the future 
        public int objectUpgradeLevel;

        //ref to call into general data
        //gets initialized inside MainBase.cs on startauth
        public Selectable selectableRef;

        private ActiveUpgradeData _activeUpgradeData;
        public ActiveUpgradeData GetActiveUpgradeData() => _activeUpgradeData;

        public readonly SyncList<int> SyncList_UpgradeLevels = new SyncList<int>();

        public readonly SyncDictionary<IdentifierType, int[]> SyncDict_ActiveUpgradeData = new SyncDictionary<IdentifierType, int[]>();
         

        private void Awake()
        {
            selectableRef = GetComponent<Selectable>();
        }

        private void OnEnable()
        {
            selectableRef.OnObjectUpgraded += OnObjectUpgraded;
        }

        private void OnDisable()
        {
            selectableRef.OnObjectUpgraded -= OnObjectUpgraded;
        }
        private void OnObjectUpgraded(int newUpgradeLevel)
        {
            objectUpgradeLevel = newUpgradeLevel;
        }

        //create a container for all the upgrade data and pass it inside syncdict
        //finish
        /// <summary>
        /// use orderId to get upgrade data from the upgradesData
        /// </summary>
        /// <param name="orderId"></param>
        public void TryUpgrade(int orderId)
        {
            if (upgradeTimerStarted) return;

            var upgradeLevel = SyncList_UpgradeLevels[orderId];
            var statUpgrade = upgradesData[orderId].upgrades[upgradeLevel];

            if (statUpgrade.prerequisiteData.UpgradeLevel > selectableRef.GetUpgradeLevel()) return;

            //used for icon rly
            var nextUpgradeExistCheck = upgradesData[orderId].upgrades.Length > (upgradeLevel + 1);
            var nextStatUpgrade = nextUpgradeExistCheck ? upgradesData[orderId].upgrades[upgradeLevel + 1] : null;

            _activeUpgradeData = new ActiveUpgradeData(orderId, upgradeLevel, statUpgrade, nextStatUpgrade);

            //start upgrade timer, initiating the upgrade chain
            SetUpgradeTimer(true, (int)statUpgrade.UpgradeTimer);

            StartCoroutine(UpgradeTimer_Coroutine((int)statUpgrade.UpgradeTimer, orderId));
        }

        IEnumerator UpgradeTimer_Coroutine(int upgradeTimer, int orderId)
        {
            yield return new WaitForSeconds(upgradeTimer);

            TryAddUpgradeIncrement(orderId);

            yield return new WaitForSeconds(syncInterval);

            Event_OnUpgrade?.Invoke(_activeUpgradeData);

            //upgrade happens here
            var upgradeLevel = SyncList_UpgradeLevels[orderId] - 1;

            if (upgradesData[orderId].upgrades.Length < upgradeLevel) yield break;

            var upgrade = upgradesData[orderId].upgrades[upgradeLevel];

            for (int i = 0; i < upgrade.upgradeData.Length; i++)
            {
                var data = upgrade.upgradeData[i];

                var identifierType = data.IdentifierType;
                var statType = data.StatType;
                var operationType = data.OperationType;
                var statValue = data.StatValue;

                switch (operationType)
                {
                    case OperationType.add:
                        SyncDict_ActiveUpgradeData[identifierType][(int)statType] += (int)statValue;
                        break;
                    case OperationType.subtract:
                        SyncDict_ActiveUpgradeData[identifierType][(int)statType] -= (int)statValue;
                        break;
                    case OperationType.equals:
                        SyncDict_ActiveUpgradeData[identifierType][(int)statType] = (int)statValue;
                        break;
                    case OperationType.multiply:
                        SyncDict_ActiveUpgradeData[identifierType][(int)statType] *= (int)statValue;
                        break;
                    case OperationType.divide:
                        SyncDict_ActiveUpgradeData[identifierType][(int)statType] /= (int)statValue;
                        break;
                }

                //print(SyncDict_UpgradeStatDataSets[identifierType][(int)statType]);
            }

            //reset timer
            SetUpgradeTimer(false, 0);
        }

        /// <summary>
        /// network timer used to control the upgradeTimer
        /// and send out client events on start/stop
        /// </summary>
        /// <param name="upgradeTimerStarted"></param>
        /// <param name="upgradeTimer"></param>
        private void SetUpgradeTimer(bool upgradeTimerStarted, int upgradeTimer)
        {
            CmdSetUpgradeTimer(upgradeTimer);
            CmdSetStartUpgradeTimer(upgradeTimerStarted);
        }
        private void TryAddUpgradeIncrement(int orderId)
        {
            CmdAddUpgradeIncrement(orderId);
        }

        [Command]
        private void CmdSetUpgradeTimer(int upgradeTimer)
        {
            ServerSetUpgradeTimer(upgradeTimer);
        }
        [Server]
        private void ServerSetUpgradeTimer(int upgradeTimer)
        {
            this.upgradeTimer = upgradeTimer;
        }

        [Command]
        private void CmdSetStartUpgradeTimer(bool upgradeTimerStarted)
        {
            ServerSetStartUpgradeTimer(upgradeTimerStarted);
        }
        [Server]
        private void ServerSetStartUpgradeTimer(bool upgradeTimerStarted)
        {
            this.upgradeTimerStarted = upgradeTimerStarted;
        }

        [Command]
        private void CmdAddUpgradeIncrement(int orderId)
        {
            ServerAddUpgradeIncrement(orderId);
        }
        [Server]
        private void ServerAddUpgradeIncrement(int orderId)
        {
            SyncList_UpgradeLevels[orderId]++;
        }

        [Server]
        private void ServerCreateUpgradeIncrements()
        {
            for (int i = 0; i < upgradesData.Length; i++)
            {
                SyncList_UpgradeLevels.Add(0);
            }
        }

        [Server]
        private void ServerCreateUpgradeDataValueSets()
        {
            var identifierValues = Enum.GetValues(typeof(IdentifierType));
            var statValues = Enum.GetValues(typeof(StatType));

            for (int i = 0; i < identifierValues.Length; i++)
            {
                var value = identifierValues.GetValue(i);

                SyncDict_ActiveUpgradeData.Add((IdentifierType)value, new int[statValues.Length]);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            ServerCreateUpgradeIncrements();
            ServerCreateUpgradeDataValueSets();
        }
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            objectUpgradeLevel = selectableRef.GetUpgradeLevel();
        }

        //syncvar hooks
        private void OnUpgradeTimerUpdated(int oldUpdateTimer, int newUpdateTimer)
        {
            Event_UpgradeTimerUpdated?.Invoke(newUpdateTimer);
        }
        private void OnUpgradeTimerStarted(bool oldUpgradeTimerStarted, bool newUpgradeTimerStarted)
        {
            Event_UpgradeTimerStarted?.Invoke(newUpgradeTimerStarted, _activeUpgradeData);
        }
    }
}
