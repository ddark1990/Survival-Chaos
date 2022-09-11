using Doozy.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SurvivalChaos
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager instance;

        UIPopup uiPopup;

        private void Start()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        public void ShowPopup(string text, Sprite icon)
        {
            uiPopup = UIPopupManager.GetPopup("InfoPopup");

            if (uiPopup == null) return;

            uiPopup.Data.SetLabelsTexts(text);
            uiPopup.Data.SetImagesSprites(icon);

            UIPopupManager.ShowPopup(uiPopup, uiPopup.AddToPopupQueue, false);
        }
        
        public void ShowPopup(string text)
        {
            uiPopup = UIPopupManager.GetPopup("InfoPopup");

            if (uiPopup == null) return;

            uiPopup.Data.SetLabelsTexts(text);

            UIPopupManager.ShowPopup(uiPopup, uiPopup.AddToPopupQueue, false);
        }
    }
}
