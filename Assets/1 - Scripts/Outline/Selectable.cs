using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.Events;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SurvivalChaos
{
    public class Selectable : GeneralData
    {
        [SyncVar(hook = nameof(OnMaterialIndexChanged))]
        [HideInInspector] public int materialIndex = -1;

        [HideInInspector] public bool selected;

        public SpriteRenderer minimapIcon;

        public event Action<int> MaterialIndexUpdated;


        private void OnMouseEnter()
        {
            SelectionManager.Instance.HoveringObject = this;
        }

/*        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            SelectionManager.OnObjectSelected?.Invoke(this);
        }
*/
        private void OnMouseExit()
        {
            if (SelectionManager.Instance.HoveringObject.Equals(this))
                SelectionManager.Instance.HoveringObject = null;
        }

        [Server]
        public void ServerSetMaterialIndex(int materialIndex)
        {
            this.materialIndex = materialIndex;
        }

        private void OnMaterialIndexChanged(int oldIndex, int newIndex)
        {
            MaterialIndexUpdated?.Invoke(newIndex);

            SetMinimapIconColors(newIndex);
        }

        public void SetMinimapIconColors(int newIndex)
        {
            switch (newIndex)
            {
                case 0:
                    minimapIcon.color = Color.green;
                    break;
                case 1:
                    minimapIcon.color = Color.red;
                    break;
                case 2:
                    minimapIcon.color = Color.blue;
                    break;
                case 3:
                    minimapIcon.color = new Color(118, 10, 85, 255);
                    break;
                default:
                    break;
            }
        }
    }
}