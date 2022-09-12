using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    [CreateAssetMenu(fileName = "BarracksDataObject_", menuName = "SurvivalChaos/Create New Barracks Data Object", order = 3)]
    public class BarracksScriptableData : GeneralDataScriptableObject
    {
        [Header("UnitData")]
        public Unit[] units;

        [Header("ColorData")]
        //public Material[] buildingColorMaterials;
        public Material[] unitColorMaterials;

    }

    public struct UnitTier
    {
        public Unit[] tierUnits;

    }
}
