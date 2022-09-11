using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;

namespace SurvivalChaos
{
    public enum IdentifierType
    {
        Melee,
        Range,
        Magic,
        Siege,
        Air,
        Artillery,
        MainBase,
        Barracks,
        Turret
    }
    public enum AttackType
    {
        Light,
        Medium,
        Heavy,
        Piercing,
        Magic,
        Hero,
        Titan
    }
    public enum DefenseType
    {
        Light,
        Medium,
        Heavy,
        Hero,
        Titan
    }

    [CreateAssetMenu(fileName = "GeneralDataObject_", menuName = "SurvivalChaos/Create New General Data Object", order = 1)]
    public class GeneralDataScriptableObject : ScriptableObject
    {
        public string objectName;

        public Sprite objectIcon;

        public StatData[] statData;

        public AttackType attackType;
        public DefenseType defenseType;

        [TextArea]
        public string description;

        [Header("Model")]
        public GameObject modelUIRender;

        public IdentifierType identifierType;

        public StatIconData statIcons;

    }

    [Serializable]
    public struct StatData
    {
        public StatType type;
        public int value;
    }
}