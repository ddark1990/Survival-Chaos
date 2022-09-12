using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class NetworkPlayerGameData : NetworkBehaviour
    {
        [SyncVar(hook = nameof(HandlePlayerGoldUpdated))]
        private int playerGold;
        public int CurrentPlayerGold() => playerGold;

        [SyncVar/*(hook = nameof(AuthorityHandlePlayerGoldUpdated))*/]
        private int playerIncome;
        public int CurrentPlayerIncome() => playerIncome;

        [SyncVar(hook = nameof(HandlePlayerScoreUpdated))]
        private int playerScore;
        public int CurrentPlayerScore() => playerScore;

        public static event Action<int> OnPlayerGoldUpdated;
        public static event Action<int> OnPlayerScoreUpdated;

        #region SyncVars

        private void HandlePlayerGoldUpdated(int oldGold, int newGold)
        {
            if (!hasAuthority) return;

            OnPlayerGoldUpdated?.Invoke(newGold);
        }
        private void HandlePlayerScoreUpdated(int oldScore, int newScore)
        {
            if (!hasAuthority) return;

            OnPlayerScoreUpdated?.Invoke(newScore);
        }

        #endregion
    }
}
