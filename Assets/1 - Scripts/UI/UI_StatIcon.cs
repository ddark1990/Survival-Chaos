using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class UI_StatIcon : MonoBehaviour
    {
        public TextMeshProUGUI statText;
        public Image iconImage;
        public StatType statType;

        private void Awake()
        {
            statText = GetComponentInChildren<TextMeshProUGUI>();
            iconImage = GetComponentInChildren<Image>();
        }

    }
}
