using Cinemachine;
using Doozy.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class TitleScreenUICameraController : MonoBehaviour
    {
        [SerializeField] List<CinemachineVirtualCamera> menuCameras = new List<CinemachineVirtualCamera>();

        private void OnEnable()
        {
            UIView.OnUIViewAction += UpdateCameras;
        }
        private void OnDisable()
        {
            UIView.OnUIViewAction -= UpdateCameras;
        }

        private void UpdateCameras(UIView view, UIViewBehaviorType type)
        {
            //print($"View:{view} | Type:{type}");

            switch (view.ViewName)
            {
                //make strings constant
                case "Main":
                    SwapCamPriority("MainMenuCam");

                    break;
                case "Host":
                    SwapCamPriority("HostScreenCam");

                    break;
                case "Join":
                    SwapCamPriority("JoinScreenCam");

                    break;
                case "Lobby":
                    SwapCamPriority("LobbyScreenCam");

                    break;

            }
        }

        private void SwapCamPriority(CinemachineVirtualCamera swapTo)
        {
            swapTo.Priority = 1;

            foreach (var cam in menuCameras)
            {
                if (cam.Equals(swapTo)) continue;

                if (cam.Priority != 0) cam.Priority = 0;
            }
        }
        //uses gameobject tag system to identify the camera to swap to
        private void SwapCamPriority(string swapToCamObjectTag)
        {
            foreach (var cam in menuCameras)
            {
                if (cam.gameObject.CompareTag(swapToCamObjectTag)) cam.Priority = 1;
                else cam.Priority = 0;
            }
        }
    }

}
