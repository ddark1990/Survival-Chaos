using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class UI_LobbyPlayerItem : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerNameText;
        [SerializeField] Image itemColor;

        NetworkPlayer _player;
        public NetworkPlayer Player => _player;
        public bool IsOccupied => _player != null;

        public void InitializeLobbyPlayerItem(NetworkPlayer player)
        {
            _player = player;

            playerNameText.text = player.PlayerName;

            var origColor = itemColor.color;
            var tempColor = player ? player.PlayerColor : origColor;

            LeanTween.value(gameObject, itemColor.color, tempColor, 1f).setOnUpdate((Color val) => {
                itemColor.color = val;
            });

        }

        public void ResetLobbyPlayerItem()
        {
            _player = null;
            playerNameText.text = "Waiting for player...";
            itemColor.color = new Color(Color.white.r, Color.white.g, Color.white.b, .2f);
        }
    }

}