using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    //might need to be owned by host player, maybe?
    public class LobbyGameStarter : NetworkBehaviour
    {
        public static LobbyGameStarter Instance;

        [SyncVar(hook = nameof(HandleStartGameTimerUpdated))]
        int _startGameTimer;
        public int StartGameTimer => _startGameTimer;

        [SyncVar(hook = nameof(HandleGameStarted))]
        bool _gameStarted;

        [SyncVar(hook = nameof(HandleGameStartedCanceled))]
        bool _gameStartedCanceled;
        public bool GameStartedCanceled => _gameStartedCanceled;

        WaitForSeconds _waitForSecond = new WaitForSeconds(1);

        public static event Action<int> OnGameStartedTimerCountdownStarted; //holds startedTime as ref
        public static event Action<int> OnGameStartedTimerCountdown;
        public static event Action OnGameStarted;
        public static event Action<bool> OnGameStartedCancel;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            _startGameTimer = 0;
            _gameStarted = false;
            _gameStartedCanceled = false;
        }

        [Command(requiresAuthority = false)]
        public void Cmd_StartGame()
        {
            StartCoroutine(GameStartedTimerCountDown(5));
        }

        [Command(requiresAuthority = false)]
        public void Cmd_CancelStartGame()
        {
            _gameStartedCanceled = true;
        }

        private IEnumerator GameStartedTimerCountDown(int timerLength)
        {
            _startGameTimer = timerLength;

            while (_startGameTimer > 0)
            {
                if (!NetworkClient.active || _gameStartedCanceled)
                {
                    _gameStartedCanceled = false;

                    yield break;
                }

                yield return _waitForSecond;
                _startGameTimer--;
            }

            _gameStarted = true;

            NetworkManager.singleton.ServerChangeScene("Game");
            //have rpc here to turn on loading screen before scene starts loading
        }

        private void HandleGameStarted(bool oldState, bool newState)
        {
            OnGameStarted?.Invoke();
        }

        private void HandleGameStartedCanceled(bool oldState, bool newState)
        {
            OnGameStartedCancel?.Invoke(newState);
        }

        private void HandleStartGameTimerUpdated(int oldTime, int newTime)
        {
            if (newTime > 4) OnGameStartedTimerCountdownStarted?.Invoke(newTime);

            OnGameStartedTimerCountdown?.Invoke(newTime);
        }
    }
}