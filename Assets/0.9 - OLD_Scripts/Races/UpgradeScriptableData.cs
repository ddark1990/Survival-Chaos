using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    [CreateAssetMenu(fileName = "UpgradeDataObject_", menuName = "SurvivalChaos/Upgrades/Create New Upgrade Data Object", order = 1)]
    public class UpgradeScriptableData : ScriptableObject
    {
        [Serializable]
        public class StatUpgrade
        {
            public enum StatType
            {
                Attack,
                AttackSpeed,
                AttackRange,
                Armor,
                Health,
                HealthRegen,
                Mana,
                ManaRegen,
                MoveSpeed,
                Evasion
            }

            public enum OperationType
            {
                add,
                subtract,
                equals,
                multiply,
                divide
            }

            public string UpgradeName;
            public Sprite Icon;
            public double UpgradeTimer;
            public UpgradeData[] upgradeData;
            public PrerequisiteData prerequisiteData;
            //public Spell[] spellsToApply;

            [Serializable]
            public struct UpgradeData
            {
                public IdentifierType IdentifierType;
                public StatType StatType;
                public OperationType OperationType;
                public double StatValue;
            }

            [Serializable]
            public struct PrerequisiteData
            {
                public int UpgradeLevel;

            }
        }

        public StatUpgrade[] upgrades;
    }
}
