using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
using Mirror;

namespace SurvivalChaos
{
    public class SteamLobby : MonoBehaviour
    {
        public static SteamLobby Instance;

        public static ulong CurrentLobbyID;

        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEnter;
        protected Callback<LobbyKicked_t> lobbyKicked;

        protected Callback<LobbyMatchList_t> lobbyList;
        protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;

        public List<CSteamID> lobbyIds = new List<CSteamID>();

        const string HOST_ADRESS = "HostAddress";

        [SerializeField] int steamLobbyCountFilter = 60;


        private void Start()
        {
            Instance = this;

            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("Steam not initialized");
                return;
            }

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyKicked = Callback<LobbyKicked_t>.Create(OnLobbyKicked);

            lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
            lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
        }

        public void GetLobbies()
        {
            if (lobbyIds.Count > 0) lobbyIds.Clear();

            SteamMatchmaking.AddRequestLobbyListResultCountFilter(steamLobbyCountFilter);
            SteamMatchmaking.RequestLobbyList();
        }

        private void OnLobbyDataUpdated(LobbyDataUpdate_t result)
        {
            if (UI_SteamLobby.Instance == null) return;
            UI_SteamLobby.Instance.PopulateSteamLobbies(lobbyIds, result);
        }

        private void OnGetLobbyList(LobbyMatchList_t result)
        {
            UI_SteamLobby.Instance.ClearAllSteamLobbies();
            //UI_Controller.Instance.OnGetLobbies?.Invoke();

            for (int i = 0; i < result.m_nLobbiesMatching; i++)
            {
                var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                lobbyIds.Add(lobbyId);
                SteamMatchmaking.RequestLobbyData(lobbyId);
            }
        }

        //callback from SteamMatchmaking.CreateLobby
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                //UI_Controller.Instance.ToggleMainCanvas(true);
                //UI_Controller.Instance.hostButton.SetActive(true);

                return;
            }

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", 
                UI_MainMenu.Instance.GameName != string.Empty ? UI_MainMenu.Instance.GameName : SteamFriends.GetPersonaName() + "'s Lobby");
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HOST_ADRESS, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "game_started", "False");
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "my_game", "True");

            GameNetworkManager.singleton.StartHost();
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            //if (NetworkServer.active) return;

            var hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HOST_ADRESS);

            GameNetworkManager.singleton.networkAddress = hostAdress;

            if (!GameNetworkManager.singleton.isNetworkActive) GameNetworkManager.singleton.StartClient();

            CurrentLobbyID = callback.m_ulSteamIDLobby;
        }

        private void OnLobbyKicked(LobbyKicked_t callback)
        {
            if (callback.m_ulSteamIDLobby != CurrentLobbyID) return;



        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        public static Texture2D GetSteamImageAsTexture(int iImage)
        {
            Texture2D texture = null;

            bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
            if (isValid)
            {
                byte[] image = new byte[width * height * 4];

                isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

                if (isValid)
                {
                    texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                }
            }

            return texture;
        }
    }
}