using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class UI_Minimap : MonoBehaviour
    {

        void Start()
        {
            StartCoroutine(FadeMinimapIn());
        }

        IEnumerator FadeMinimapIn()
        {
            yield return new WaitUntil(() => CameraControl.instance);
            yield return new WaitUntil(() => CameraControl.instance.controlsEnabled);

            var mapColor = GetComponent<RawImage>().color;

            /*switch (((GameNetworkManagerOld)NetworkManager.singleton).localRoomPlayerRef.playerId)
            {
                case 0:
                    transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
                case 1:
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 2:
                    transform.rotation = Quaternion.Euler(0, 0, -90);
                    break;
                case 3:
                    transform.rotation = Quaternion.Euler(0, 0, -180);
                    break;
            }*/

            LeanTween.value(gameObject, mapColor, Color.white, 1f).setOnUpdate((Color val) => {
                GetComponent<RawImage>().color = val;
            });
        }
    }

}