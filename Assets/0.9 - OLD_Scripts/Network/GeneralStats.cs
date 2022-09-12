using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class GeneralStats
    {
        #region SyncVars
        [SyncVar(hook = nameof(OnNameUpdated))]
        private string objectName;

        [SyncVar(hook = nameof(OnUpgradeLevelUpdated))]
        private int upgradeLevel = -1;

        //finish server methods
        [SyncVar/*(hook = nameof(OnObjectUpgradeLevelUpdated))*/]
        private float upgradeTimer;

        [SyncVar(hook = nameof(OnDeathUpdated))]
        private bool isDead;

        [SyncVar(hook = nameof(OnHealthUpdated))]
        private int objectCurrentHealth;

        [SyncVar(hook = nameof(OnManaUpdated))]
        private int objectMana;

        [SyncVar(hook = nameof(OnMaxHealthUpdated))]
        private int objectMaxHealth;
        public int GetMaxHealth() => objectMaxHealth;

        [SyncVar(hook = nameof(OnMaxManaUpdated))]
        private int objectMaxMana;

        [SyncVar(hook = nameof(OnAttackUpdated))]
        private int objectAttack;

        [SyncVar(hook = nameof(OnDefenseUpdated))]
        private int objectDefense;

        [SyncVar(hook = nameof(OnAttackLevelUpdated))]
        private int objectAttackLevel;

        [SyncVar(hook = nameof(OnDefenseLevelUpdated))]
        private int objectDefenseLevel;

        [SyncVar(hook = nameof(OnAttackRateUpdated))]
        private float attackRate;

        [SyncVar(hook = nameof(OnAttackRangeUpdated))]
        private float attackRange;
        #endregion

        #region Events
        public event Action<string> OnNameChanged;
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<float, float> OnMaxHealthChanged;
        public event Action<float, float> OnMaxManaChanged;
        public event Action<Selectable> OnDeath;

        public event Action<Selectable> OnTarget;
        public event Action OnTargetClear;

        public event Action OnAttack;
        public event Action<Unit> OnUnitStopAttackAnimation;
        #endregion

        #region Getters
        public string GetName() => objectName;
        public int GetUpgradeLevel() => upgradeLevel;
        public float GetUpgradeTimer() => upgradeTimer;
        public bool IsDead() => isDead;
        public int GetCurrentHealth() => objectCurrentHealth;
        public int GetCurrentMana() => objectMana;
        public int GetMaxMana() => objectMaxMana;
        public int GetAttack() => objectAttack;
        public int GetDefense() => objectDefense;
        public int GetAttackLevel() => objectAttackLevel;
        public int GetDefenseLevel() => objectDefenseLevel;
        public float GetAttackRate() => attackRate;
        public float GetAttackRange() => attackRange;
        #endregion

        #region Hooks
        [Server]
        public void ServerSetObjectName(string name)
        {
            objectName = name;
        }
        private void OnNameUpdated(string oldName, string newName)
        {
            OnNameChanged?.Invoke(newName);
        }

        [Server]
        public void ServerSetUpgradeLevel(int upgradeLevel)
        {
            this.upgradeLevel = upgradeLevel;
        }
        //could call commands here for server auth
        private void OnUpgradeLevelUpdated(int oldUpgradeLevel, int newUpgradeLevel)
        {

        }

        [Server]
        public void ServerSetDeathState(bool isDead)
        {
            this.isDead = isDead;
        }
        private void OnDeathUpdated(bool oldDeathState, bool newDeathState)
        {

        }

        [Server]
        public int ServerSetHealth(int newHealth)
        {
            return objectCurrentHealth = newHealth;
        }
        private void OnHealthUpdated(int oldHealth, int newHealth)
        {
            OnHealthChanged?.Invoke(newHealth, GetMaxHealth());
        }

        [Server]
        public void ServerSetMana(int newMana)
        {
            objectMana = newMana;
        }
        private void OnManaUpdated(int oldMana, int newMana)
        {
            OnManaChanged?.Invoke(newMana, GetMaxMana());
        }

        [Server]
        public void ServerSetMaxHealth(int newMaxHealth)
        {
            objectMaxHealth = newMaxHealth;
        }
        private void OnMaxHealthUpdated(int oldMaxHealth, int newMaxHealth)
        {
            OnMaxHealthChanged?.Invoke(GetCurrentHealth(), newMaxHealth);
        }

        [Server]
        public void ServerSetMaxMana(int newMaxMana)
        {
            objectMaxMana = newMaxMana;
        }
        private void OnMaxManaUpdated(int oldMaxMana, int newMaxMana)
        {
            OnMaxManaChanged?.Invoke(GetCurrentMana(), newMaxMana);
        }

        [Server]
        public void ServerSetAttack(int newAttack)
        {
            objectAttack = newAttack;
        }
        private void OnAttackUpdated(int oldAttack, int newAttack)
        {

        }

        [Server]
        public void ServerSetDefense(int newDefense)
        {
            objectDefense = newDefense;
        }
        private void OnDefenseUpdated(int oldDefense, int newDefense)
        {

        }

        [Server]
        public void ServerSetAttackLevel(int newAttack)
        {
            objectAttack = newAttack;
        }
        private void OnAttackLevelUpdated(int oldAttack, int newAttack)
        {

        }

        [Server]
        public void ServerSetDefenseLevel(int newDefenseLevel)
        {
            objectDefense = newDefenseLevel;
        }
        private void OnDefenseLevelUpdated(int oldDefenseLevel, int newDefenseLevel)
        {

        }

        [Server]
        public void ServerSetAttackRate(float newAttackRate)
        {
            attackRate = newAttackRate;
        }
        private void OnAttackRateUpdated(float oldAttackRate, float newAttackRate)
        {

        }

        [Server]
        public void ServerSetAttackRange(float newAttackRange)
        {
            attackRange = newAttackRange;
        }
        private void OnAttackRangeUpdated(float oldAttackRange, float newAttackRange)
        {

        }

        [Server]
        public void ServerUpgradeBarracks()
        {
            if (upgradeLevel == 3) return;
            upgradeLevel++;
        }
        #endregion

    }
}
