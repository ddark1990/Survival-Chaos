using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;

namespace SurvivalChaos
{
    public class UI_DynamicTextElement : MonoBehaviour
    {
        [HideInInspector] public TextMeshProUGUI textMeshProUGUI;
        public StatType statType;

        private void Awake()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }
    }
}
