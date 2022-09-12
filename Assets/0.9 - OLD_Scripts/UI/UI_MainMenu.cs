using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class UI_MainMenu : MonoBehaviour
    {
        public static UI_MainMenu Instance;

        [SerializeField] InputField gameNameInputField;
        public string GameName => gameNameInputField.text;

        [SerializeField] TMP_Dropdown lobbyTypeDropDown;

        private void Awake()
        {
            Instance = this;
        }

        public void OnButtonPress_CreateGame()
        {
            ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic;

            switch (lobbyTypeDropDown.options[lobbyTypeDropDown.value].text)
            {
                case "Public":
                    lobbyType = ELobbyType.k_ELobbyTypePublic;
                    break;
                case "Private":
                    lobbyType = ELobbyType.k_ELobbyTypePrivate;
                    break;
                case "FriendsOnly":
                    lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
                    break;
            }

            SteamMatchmaking.CreateLobby(lobbyType, GameNetworkManager.singleton.maxConnections);
        }

        public void OnButtonPress_QuitGame()
        {
            Application.Quit();
        }
    }
}
