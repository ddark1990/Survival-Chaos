using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivalChaos
{
    //selection indicator & hover over healthbar are the only things this script does right now
    public class Selectable_WorldUI_Controller : MonoBehaviour
    {
        [SerializeField] Canvas selectionCanvas;
        [SerializeField] Image selectionCircle;
        [SerializeField] Image worldHealthBar;

        Selectable selectable;

        private void OnEnable()
        {
            SelectionManager.OnObjectSelected += SetUnitSelection;
            SelectionManager.OnObjectDeselected += ClearUnitSelection;

            selectable.GetComponent<GeneralData>().OnObjectHealthChanged += UI_SetWorldHealthBar;
        }

        private void OnDisable()
        {
            SelectionManager.OnObjectSelected -= SetUnitSelection;
            SelectionManager.OnObjectDeselected -= ClearUnitSelection;

            selectable.GetComponent<GeneralData>().OnObjectHealthChanged -= UI_SetWorldHealthBar;
        }

        private void UI_SetWorldHealthBar(float health, float maxHealth)
        {
            if (worldHealthBar == null) return;

            worldHealthBar.fillAmount = SelectionUI.GetNormalizedValue(health, maxHealth);
        }

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        private void OnMouseEnter()
        {
            if (selectable.selected || EventSystem.current.IsPointerOverGameObject()) return;

            //set up central component for general things like getters for rigidbody, collider, etc..
            selectionCanvas.GetComponent<RectTransform>().sizeDelta = selectable.GetComponent<Unit>() ?
                new Vector2(selectable.GetComponentInChildren<Renderer>().bounds.extents.magnitude,
                selectable.GetComponentInChildren<Renderer>().bounds.extents.magnitude) :
                new Vector2(selectable.GetComponentInChildren<Renderer>().bounds.size.magnitude,
                selectable.GetComponentInChildren<Renderer>().bounds.size.magnitude);

            selectionCircle.gameObject.SetActive(true);
            if (worldHealthBar) worldHealthBar.gameObject.SetActive(true);
        }

        private void OnMouseExit()
        {
            if (selectable.selected) return;

            selectionCircle.gameObject.SetActive(false);
            if(worldHealthBar) worldHealthBar.gameObject.SetActive(false);
        }

        public void SetUnitSelection(Selectable selectable)
        {
            if (selectable != this.selectable) return;

            selectionCircle.color = SelectionManager.Instance.selectedColor;

            selectionCircle.gameObject.SetActive(true);
            if (worldHealthBar) worldHealthBar.gameObject.SetActive(true);
        }

        public void ClearUnitSelection(Selectable selectable)
        {
            selectionCircle.gameObject.SetActive(false);
            if (worldHealthBar) worldHealthBar.gameObject.SetActive(false);

            selectionCircle.color = SelectionManager.Instance.hoverOverColor;
        }
    }
}
