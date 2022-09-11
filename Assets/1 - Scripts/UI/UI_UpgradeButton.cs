using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static SurvivalChaos.UpgradeScriptableData;

namespace SurvivalChaos
{
    public class UI_UpgradeButton : MonoBehaviour 
    {
        public int orderId;

        [Header("ref")]
        public Image iconImage;
        public TextMeshProUGUI countText;
        public Image coolDownTimer;
    }
}
