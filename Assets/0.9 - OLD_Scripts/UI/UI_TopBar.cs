using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace SurvivalChaos
{
    public class UI_TopBar : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerGoldText;
        [SerializeField] TextMeshProUGUI playerScoreText;

        private void OnEnable()
        {
            NetworkPlayerOLD.AuthorityOnPlayerGoldUpdated += UpdateGoldText;
            NetworkPlayerOLD.AuthorityOnPlayerScoreUpdated += UpdateScoreText;
        }

        private void OnDisable()
        {
            NetworkPlayerOLD.AuthorityOnPlayerGoldUpdated -= UpdateGoldText;
            NetworkPlayerOLD.AuthorityOnPlayerScoreUpdated -= UpdateScoreText;
        }

        private void UpdateScoreText(double scoreAmount)
        {
            playerScoreText.text = scoreAmount.ToString();
        }

        private void UpdateGoldText(double goldAmount)
        {
            playerGoldText.text = goldAmount.ToString();
        }

    }
}
