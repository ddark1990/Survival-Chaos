using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class LobbyGameStarter : NetworkBehaviour
    {
        public static LobbyGameStarter Instance;

        [SyncVar(hook = nameof(HandleStartGameTimerUpdated))]
        int _startGameTimer;

        [SyncVar(hook = nameof(HandleGameStarted))]
        bool _gameStarted;

        WaitForSeconds _waitForSecond = new WaitForSeconds(1);

        public static event Action<int> OnGameStartedTimerCountdown;

        private void Awake()
        {
            Instance = this;
        }

        [Command(requiresAuthority = false)]
        public void Cmd_StartGame() //TODO: fix button interactability when starting game
        {
            StartCoroutine(GameStartedTimerCountDown(6));
        }

        private IEnumerator GameStartedTimerCountDown(int timerLength)
        {
            _startGameTimer = timerLength;

            while (_startGameTimer > -1)
            {
                if (!NetworkClient.active) yield break;

                yield return _waitForSecond;
                _startGameTimer--;
            }

            _gameStarted = true;

            NetworkManager.singleton.ServerChangeScene("Game");
            //have rpc here to turn on loading screen before scene starts loading
        }

        private void HandleGameStarted(bool oldState, bool newState)
        {

        }

        private void HandleStartGameTimerUpdated(int oldTime, int newTime)
        {
            OnGameStartedTimerCountdown?.Invoke(newTime);
        }
    }
}