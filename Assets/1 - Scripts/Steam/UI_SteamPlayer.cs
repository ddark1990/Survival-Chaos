using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks;

namespace SurvivalChaos
{
    public class UI_SteamPlayer : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerNameText;
        [SerializeField] RawImage playerIconImage;

        private void Start()
        {
            SetSteamPlayerNameText();
            SetSteamPlayerSteamIcon();
        }

        public void SetSteamPlayerNameText()
        {
            playerNameText.text = SteamFriends.GetPersonaName();
        }

        public void SetSteamPlayerSteamIcon()
        {
            var cSteamId = new CSteamID(SteamUser.GetSteamID().m_SteamID);

            var imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);

            playerIconImage.texture = SteamLobby.GetSteamImageAsTexture(imageId);
        }

    }
}
