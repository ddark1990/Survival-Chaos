using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using Doozy.Engine.Nody;

namespace SurvivalChaos
{
    public class JoinMenu : MonoBehaviour
    {
        public static JoinMenu instance;

        public TMP_InputField nameInput;
        [SerializeField] TMP_InputField ipInput;
        [SerializeField] Button joinButton;
        GraphController graphController;

        private void OnEnable()
        {
            instance = this;

            RoomPlayer.ClientOnConnected += HandleClientConnected;
            RoomPlayer.ClientOnDisconnected += HandleClientDisconnected;

            graphController = FindObjectOfType<GraphController>();
        }

        private void OnDisable()
        {
            RoomPlayer.ClientOnConnected -= HandleClientConnected;
            RoomPlayer.ClientOnDisconnected -= HandleClientDisconnected;
        }

        public void Join()
        {
            if (nameInput.text == string.Empty)
            {
                print("Please enter a name...");
                return;
            }

            if(ipInput.text == string.Empty)
            {
                print("Please enter an ip...");
                return;
            }
            print(GameNetworkManager.singleton);
            GameNetworkManager.singleton.networkAddress = ipInput.text;
            GameNetworkManager.singleton.StartClient();

            joinButton.interactable = false;
        }

        private void HandleClientConnected()
        {
            joinButton.interactable = true;
            var lobbyGraphNode = graphController.Graph.GetNodeByName("Lobby Screen");

            //if check is not needed if the UIView get disabled on switch, better safe
            if (!graphController.Graph.ActiveNode.Equals(lobbyGraphNode)) graphController.GoToNodeByName("Lobby Screen");
        }

        private void HandleClientDisconnected(RoomPlayer player)
        {
            joinButton.interactable = true;
        }
    }
}