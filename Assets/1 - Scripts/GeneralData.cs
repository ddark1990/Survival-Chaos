using Mirror;
using Pathfinding;
using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;

namespace SurvivalChaos
{
    public abstract class GeneralData : NetworkBehaviour
    {
        #region Targeter
        [Header("Targeter")]
        [SerializeField] LayerMask agroMask;
        [SerializeField] private Selectable target;
        public Selectable GetTarget() => target;
        public bool inRange;
        Collider[] hitColliders;
        //TODO:network this, and show it de wey
        float agroRadius = 7;
        #endregion

        #region CombatController
        [Header("CombatController")]
        //should be 0 at start so he attack as soon as inRange
        float attackTimer;

        #endregion

        #region Reference
        [Header("Reference")]
        public GeneralDataScriptableObject scriptableObjectData;

        #endregion

        private CapsuleCollider objectCollider;
        public CapsuleCollider GetUnitCollider() => objectCollider;

        //used for reseting all enemies targeting that are targeting this object upon death
        [HideInInspector] public List<Selectable> attackingObjects = new List<Selectable>();

        //temp
        public GameObject owner;

        #region SyncVars
        [SyncVar]
        private int ownerPlayerId;

        //network variables & hooks
        [SyncVar(hook = nameof(OnObjectNameUpdated))]
        private string objectName;

        [SyncVar(hook = nameof(OnObjectUpgradeLevelUpdated))]
        private int upgradeLevel = -1;

        [SyncVar(hook = nameof(OnObjectDeathUpdated))]
        private bool isDead;

        [SyncVar(hook = nameof(OnObjectHealthUpdated))]
        private int objectHealth;

        [SyncVar(hook = nameof(OnObjectManaUpdated))]
        private int objectMana;

        [SyncVar(hook = nameof(AuthOnObjectMaxHealthUpdated))]
        private int objectMaxHealth;

        [SyncVar(hook = nameof(AuthOnObjectMaxManaUpdated))]
        private int objectMaxMana;

        [SyncVar(hook = nameof(OnObjectAttackUpdated))]
        private int objectAttack;

        [SyncVar(hook = nameof(OnObjectDefenseUpdated))]
        private int objectDefense;

        [SyncVar(hook = nameof(OnObjectAttackLevelUpdated))]
        private int objectAttackLevel;

        [SyncVar(hook = nameof(OnObjectDefenseLevelUpdated))]
        private int objectDefenseLevel;

        [SyncVar(hook = nameof(OnObjectAttackRateUpdated))]
        private float attackRate;

        [SyncVar(hook = nameof(OnObjectAttackRangeUpdated))]
        private float attackRange;

        #region SyncVarGetters
        public int GetownerPlayerId() => ownerPlayerId;
        public string GetName() => objectName;
        public int GetUpgradeLevel() => upgradeLevel;
        public bool IsDead() => isDead;
        public int GetMaxHealth() => objectMaxHealth;
        public int GetMaxMana() => objectMaxMana;

        public int GetCurrentHealth() => objectHealth;
        public int GetCurrentMana() => objectMana;
        public int GetAttack() => objectAttack;
        public int GetDefense() => objectDefense;
        public int GetAttackLevel() => objectAttackLevel;
        public int GetDefenseLevel() => objectDefenseLevel;
        public float GetAttackRate() => attackRate;
        public float GetAttackRange() => attackRange;
        #endregion

        //public readonly SyncList<int> SyncList_StatData = new SyncList<int>();
        public readonly SyncDictionary<StatType, int> SyncDict_ActiveStatData = new SyncDictionary<StatType, int>();

        #endregion

        #region Events
        public event Action<string> OnObjectNameChanged;
        public event Action<float, float> OnObjectHealthChanged;
        public event Action<float, float> OnObjectManaChanged;
        public event Action<float, float> OnObjectMaxHealthChanged;
        public event Action<float, float> OnObjectMaxManaChanged;
        public event Action<Selectable> OnObjectDeath;

        public event Action<int> OnObjectUpgraded;

        public event Action<Selectable> OnTarget;
        public event Action OnTargetClear;

        public event Action OnAttack;
        public event Action<Unit> OnUnitStopAttackAnimation;
        #endregion

        private void Update()
        {
            UpdateControllers();
        }

        /// <summary>
        /// Update general system controllers. Combat Controller - Targeter
        /// </summary>
        private void UpdateControllers()
        {
            //if were dead do nothing
            if (IsDead()) return;

            //if target is dead, clear our target
            if (target != null && target.IsDead()) ServerClearTarget();

            //if we have a target, do not update the targeter
            //and start updating the combat controller
            if (target != null)
            {
                UpdateCombatController();
                return;
            }

            UpdateTargeter();
        }

        #region ITarget
        private void UpdateTargeter()
        {
            if (!hasAuthority) return;

            //non allocating version to spherecast
            //var ray = new Ray(transform.position, transform.forward);
            //var hits = new RaycastHit[1];
            //var numberOfHits = Physics.SphereCastNonAlloc(ray, agroRadius, hits, 0, agroMask);

            //pre sure has some garbage allocation
            hitColliders = Physics.OverlapSphere(transform.position, agroRadius, agroMask);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                var hit = hitColliders[i];

                //targets first unit that it sees, change to a burst, show it de wey
                var selecatable = hit.GetComponent<Selectable>();
                var hasAuthority = selecatable.hasAuthority;

                //dont target if we own the object
                if (hasAuthority) continue;

                TryTarget(selecatable);
            }
        }

        public void TryTarget(Selectable selectable)
        {
            CmdSetTarget(selectable.gameObject);
        }
        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject) return;

            if (!targetGameObject.TryGetComponent<Selectable>(out Selectable newTarget)) { return; }

            //set target on own client
            target = newTarget;
            //and on and other players clients
            RpcSetTarget(targetGameObject);

            //add self to the targets attackers list
            RpcAddToAttackingList(newTarget);

            OnTarget?.Invoke(newTarget);
        }
        [ClientRpc]
        public void RpcAddToAttackingList(Selectable newTarget)
        {
            if (newTarget == null) return;

            newTarget.attackingObjects.Add(newTarget);
        }
        [ClientRpc]
        public void RpcSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject) return;

            if (!targetGameObject.TryGetComponent<Selectable>(out Selectable newTarget)) { return; }

            target = newTarget;
        }
        [Server]
        public void ServerClearTarget()
        {
            ClientClearTarget();
            RpcClearTarget();
        }
        [ClientRpc]
        public void RpcClearTarget()
        {
            ClientClearTarget();
        }
        private void ClientClearTarget()
        {
            target = null;

            if (target == null) OnTargetClear?.Invoke();
        }
        #endregion

        #region ICombat
        private void UpdateCombatController()
        {
            //if not in range, do nothing
            if (!inRange) return;

            TryAttack(target);
        }
        public void TryAttack(Selectable target)
        {
            var targetUnit = target.GetComponent<Unit>();

            //might not need cuz we check for this already from earlier execution
            //if (targetUnit.IsDead()) return;

            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                OnAttack?.Invoke();

                targetUnit.CmdApplyDamage(GetAttack());

                attackTimer = GetAttackRate();
            }
        }

        #endregion

        #region Server
        public override void OnStartServer()
        {
            base.OnStartServer();

            InitializeGeneralStatData();
        }

        //create initial base stats from the GeneralDataScriptableObject data
        private void InitializeGeneralStatData()
        {
            foreach (var stat in scriptableObjectData.statData)
            {
                SyncDict_ActiveStatData.Add(stat.type, stat.value);
            }
        }

        #region Hooks
        [Server]
        public void ServerSetMainBaseOwner(int ownerPlayerId)
        {
            this.ownerPlayerId = ownerPlayerId;
        }

        [Server]
        public void ServerSetObjectName(string name)
        {
            objectName = name;
        }
        private void OnObjectNameUpdated(string oldName, string newName)
        {
            OnObjectNameChanged?.Invoke(newName);
        }

        [Command]
        public void CmdSetUpgradeLevel(int upgradeLevel)
        {
            ServerSetUpgradeLevel(upgradeLevel);
        }
        [Server]
        public void ServerSetUpgradeLevel(int upgradeLevel)
        {
            this.upgradeLevel = upgradeLevel;
        }
        //could call commands here for server auth
        private void OnObjectUpgradeLevelUpdated(int oldUpgradeLevel, int newUpgradeLevel)
        {
            OnObjectUpgraded?.Invoke(newUpgradeLevel);
        }

        [Server]
        public void ServerSetDeathState(bool isDead)
        {
            this.isDead = isDead;
        }
        private void OnObjectDeathUpdated(bool oldDeathState, bool newDeathState)
        {

        }

        [Server]
        public int ServerSetHealth(int newHealth)
        {
            return objectHealth = newHealth;
        }
        private void OnObjectHealthUpdated(int oldHealth, int newHealth)
        {
            OnObjectHealthChanged?.Invoke(newHealth, GetMaxHealth());
        }

        [Server]
        public void ServerSetMana(int newMana)
        {
            objectMana = newMana;
        }
        private void OnObjectManaUpdated(int oldMana, int newMana)
        {
            OnObjectManaChanged?.Invoke(newMana, GetMaxMana());
        }

        [Server]
        public void ServerSetMaxHealth(int newMaxHealth)
        {
            objectMaxHealth = newMaxHealth;
        }
        private void AuthOnObjectMaxHealthUpdated(int oldMaxHealth, int newMaxHealth)
        {
            if (!hasAuthority) return;

            OnObjectMaxHealthChanged?.Invoke(GetCurrentHealth(), newMaxHealth);
        }

        [Server]
        public void ServerSetMaxMana(int newMaxMana)
        {
            objectMaxMana = newMaxMana;
        }
        private void AuthOnObjectMaxManaUpdated(int oldMaxMana, int newMaxMana)
        {
            if (!hasAuthority) return;

            OnObjectMaxManaChanged?.Invoke(GetCurrentMana(), newMaxMana);
        }

        [Server]
        public void ServerSetAttack(int newAttack)
        {
            objectAttack = newAttack;
        }
        private void OnObjectAttackUpdated(int oldAttack, int newAttack)
        {
            
        }
        
        [Server]
        public void ServerSetDefense(int newDefense)
        {
            objectDefense = newDefense;
        }
        private void OnObjectDefenseUpdated(int oldDefense, int newDefense)
        {
            
        }

        [Server]
        public void ServerSetAttackLevel(int newAttack)
        {
            objectAttack = newAttack;
        }
        private void OnObjectAttackLevelUpdated(int oldAttack, int newAttack)
        {

        }

        [Server]
        public void ServerSetDefenseLevel(int newDefenseLevel)
        {
            objectDefense = newDefenseLevel;
        }
        private void OnObjectDefenseLevelUpdated(int oldDefenseLevel, int newDefenseLevel)
        {
            
        }

        [Server]
        public void ServerSetAttackRate(float newAttackRate)
        {
            attackRate = newAttackRate;
        }
        private void OnObjectAttackRateUpdated(float oldAttackRate, float newAttackRate)
        {

        }
        
        [Server]
        public void ServerSetAttackRange(float newAttackRange)
        {
            attackRange = newAttackRange;
        }
        private void OnObjectAttackRangeUpdated(float oldAttackRange, float newAttackRange)
        {

        }

        [Server]
        public void ServerUpgradeObject()
        {
            upgradeLevel++;
        }
        [Command]
        public void CmdUpgradeObject()
        {
            ServerUpgradeObject();
        }
        #endregion

        #endregion

        #region Damage Logic
        [Command]
        public void CmdApplyDamage(int damageToApply)
        {
            if (GetCurrentHealth() <= 0) return;

            ServerSetHealth(Mathf.Max(GetCurrentHealth() - damageToApply, 0));

            if (GetCurrentHealth() > 0) return;

            TryDie();
        }

        #endregion

        #region Death Logic
        public void TryDie()
        {
            OnObjectDeath?.Invoke(GetComponent<Selectable>());

            ServerSetDeathState(true);

            if (attackingObjects.Count > 0)
            {
                foreach (var attacker in attackingObjects)
                {
                    if (attacker == null) continue;

                    attacker.ServerClearTarget();
                }
            }

            //fully disable unit on death so he cant interfere with anything on the field
            RpcDisableDeadUnit();

            //ServerClearTarget();
        }
        [ClientRpc]
        private void RpcDisableDeadUnit()
        {
            if (!GetComponent<Unit>()) return;

            GetComponent<Unit>().GetUnitMovement().StopAgentMovement(true);
            GetComponent<Unit>().GetAgent().enabled = false;
            GetComponent<RVOController>().enabled = false;

            GetComponent<Unit>().GetRigidbody().isKinematic = true;
            GetUnitCollider().enabled = false;

        }
        #endregion

        public virtual void CacheGeneralDataComponents()
        {
            TryGetComponent(out objectCollider);
        }

        public MainBase GetMainBase()
        {
            var mainBases = FindObjectsOfType<MainBase>();

            MainBase mainBase = null;

            foreach (var item in mainBases)
            {
                if (!item.hasAuthority) continue;

                mainBase = item;
            }

            return mainBase;
        }
    }
}
