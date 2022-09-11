using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

namespace SurvivalChaos
{
    public class ChatBehaviour : NetworkBehaviour
    {
        [SerializeField] GameObject chatUi;
        [SerializeField] TextMeshProUGUI chatText;
        [SerializeField] InputField chatInputField;
        [SerializeField] int maxLines = 35;

        public RoomPlayer roomPlayerRef;

        static event Action<string> OnMessage;

        private List<string> chatList = new List<string>();

        public override void OnStartAuthority()
        {
            OnMessage += HandleMessage;

            chatUi.SetActive(true);
        }

        [ClientCallback]
        private void OnDestroy()
        {
            if (!hasAuthority) return;

            OnMessage -= HandleMessage;
        }

        void HandleMessage(string message)
        {
            Canvas.ForceUpdateCanvases();

            if (chatList.Count == maxLines)
            {
                chatList.RemoveAt(0);
            }

            chatList.Add(message);

            UpdateText();

            chatInputField.ActivateInputField();
        }


        string wholeText;
        public void UpdateText()
        {
            wholeText = string.Empty;
            foreach (string line in chatList)
            {
                wholeText = (wholeText + line);
            }
            chatText.text = wholeText;
        }

        [Client]
        public void Send(string message)
        {
            if (!Input.GetKeyDown(KeyCode.Return)) return;
            if (string.IsNullOrWhiteSpace(message)) return;

            CmSendMessage(chatInputField.text);

            chatInputField.text = string.Empty;
        }

        [Command]
        void CmSendMessage(string message)
        {
            //add protection logic here
            RpcHandleMessage($"{ColorString(roomPlayerRef.playerName, roomPlayerRef.playerColor)}: {message}");
        }

        [ClientRpc]
        void RpcHandleMessage(string message)
        {
            OnMessage?.Invoke($"\n{message}");
        }

        public static string ColorString(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
        }

    }
}