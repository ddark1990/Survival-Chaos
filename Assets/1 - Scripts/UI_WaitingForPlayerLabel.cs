using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class UI_WaitingForPlayerLabel : MonoBehaviour
    {
        [SerializeField] Image labelImage;
        Color tempColor;

        private void OnEnable()
        {
            tempColor = labelImage.color;
            RoomPlayer.ClientOnDisconnected += TweenLabelColor;

        }

        private void OnDisable()
        {
            RoomPlayer.ClientOnDisconnected -= TweenLabelColor;

        }

        private void TweenLabelColor(RoomPlayer player)
        {
            LeanTween.value(gameObject, labelImage.color, tempColor, 1f).setOnUpdate((Color val) => {
                labelImage.color = val;
            });
        }
    }
}
