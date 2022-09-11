using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;

namespace SurvivalChaos
{
    [CreateAssetMenu(fileName = "Stat_IconData_", menuName = "SurvivalChaos/Icons/Create New Stat Icon Data")]
    public class StatIconData : ScriptableObject
    {
        //has to be in order of the enum
        public IconData[] iconData;
    }

    [Serializable]
    public struct IconData
    {
        public StatType statType;
        public Sprite icon;
    }
}
