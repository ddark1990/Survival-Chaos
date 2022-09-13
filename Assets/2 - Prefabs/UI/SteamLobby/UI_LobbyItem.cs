using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

namespace SurvivalChaos
{
    public class UI_LobbyItem : MonoBehaviour
    {
        [Header("ref")]
        [SerializeField] TextMeshProUGUI lobbyNameText;
        [SerializeField] TextMeshProUGUI gameStartedText;

        private string _lobbyName;
        private CSteamID _lobbyId;

        public void InitializeLobbyItem(CSteamID lobbyId)
        {
            _lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");

            _lobbyId = lobbyId;
            lobbyNameText.text = _lobbyName /*!= string.Empty ? _lobbyName : "Unnamed Lobby"*/;

            if (SteamMatchmaking.GetLobbyData(lobbyId, "game_started") == "True")
            {
                gameStartedText.text = "In Game";
            }
            else if (SteamMatchmaking.GetLobbyData(lobbyId, "game_started") == "False")
            {
                gameStartedText.text = "In Lobby";
            }

            GetComponent<Button>().onClick.AddListener(() =>
            {
                SteamMatchmaking.JoinLobby(lobbyId);
            });
        }
    }

}