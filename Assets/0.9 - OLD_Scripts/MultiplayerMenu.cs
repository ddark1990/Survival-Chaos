using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using Doozy.Engine.UI;
using Doozy.Engine.Nody;

namespace SurvivalChaos
{
    public class MultiplayerMenu : MonoBehaviour
    {
        public static MultiplayerMenu instance;

        public TMP_InputField nameInput;
        GraphController graphController;


        private void Awake()
        {
            instance = this;

            graphController = FindObjectOfType<GraphController>();
        }

        private void OnEnable()
        {
            NetworkPlayerOLD.ClientOnConnected += HandleClientConnected;
            NetworkPlayerOLD.ClientOnDisconnected += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            NetworkPlayerOLD.ClientOnConnected -= HandleClientConnected;
            NetworkPlayerOLD.ClientOnDisconnected -= HandleClientDisconnected;
        }

        public void Host()
        {
            if(nameInput.text == string.Empty)
            {
                print("Please enter a name...");
                return;
            }
            GameNetworkManager.singleton.StartHost();
        }

        private void HandleClientConnected()
        {
            var lobbyGraphNode = graphController.Graph.GetNodeByName("Lobby Menu");

            //if check is not needed if the UIView get disabled on switch, better safe then sorry
            if(!graphController.Graph.ActiveNode.Equals(lobbyGraphNode)) graphController.GoToNodeByName("Lobby Menu");
        }

        private void HandleClientDisconnected(NetworkPlayerOLD player)
        {
            print("--Client Disconnected--");
        }
    }
}