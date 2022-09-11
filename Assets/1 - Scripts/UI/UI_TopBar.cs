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
            RoomPlayer.AuthorityOnPlayerGoldUpdated += UpdateGoldText;
            RoomPlayer.AuthorityOnPlayerScoreUpdated += UpdateScoreText;
        }

        private void OnDisable()
        {
            RoomPlayer.AuthorityOnPlayerGoldUpdated -= UpdateGoldText;
            RoomPlayer.AuthorityOnPlayerScoreUpdated -= UpdateScoreText;
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
