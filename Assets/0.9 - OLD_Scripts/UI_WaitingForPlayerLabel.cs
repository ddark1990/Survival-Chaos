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
            NetworkPlayerOLD.ClientOnDisconnected += TweenLabelColor;

        }

        private void OnDisable()
        {
            NetworkPlayerOLD.ClientOnDisconnected -= TweenLabelColor;

        }

        private void TweenLabelColor(NetworkPlayerOLD player)
        {
            LeanTween.value(gameObject, labelImage.color, tempColor, 1f).setOnUpdate((Color val) => {
                labelImage.color = val;
            });
        }
    }
}
