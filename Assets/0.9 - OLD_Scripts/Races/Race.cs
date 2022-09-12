using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    [CreateAssetMenu(fileName = "Race_", menuName = "SurvivalChaos/Create New Race", order = 1)]
    public class Race : ScriptableObject
    {
        [Header("General")]
        public string raceName;

        [Header("MainBase")]
        public GameObject mainBase;
        [Header("Barracks")]
        public GameObject barracks;
        [Header("Turret")]
        public GameObject turret;
    }
}