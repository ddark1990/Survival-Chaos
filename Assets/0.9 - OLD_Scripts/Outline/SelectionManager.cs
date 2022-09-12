using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;

namespace SurvivalChaos
{
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        //public Selectable CurrentlySelectedObject;
        public List<Selectable> CurrentlySelectedObjects = new List<Selectable>();
        public Selectable HoveringObject;
        
        [Header("Selectables")]
        public LayerMask selectableMask;
        
        public Color hoverOverColor;
        public Color selectedColor;
        public Color selectionBoxColor;

        public static Selectable LastSelectedObject;

        [HideInInspector] public Camera cam;

        [Header("SelectionBox")]
        [SerializeField] RectTransform selectionBoxGraphic;

        Rect selectionBox;

        Vector2 startDragPosition;
        Vector2 endDragPosition;

        public static Action<Selectable> OnObjectSelected { get; set; }
        public static Action<Selectable> OnObjectDeselected { get; set; }

        private void InitializeSelectionManager()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            cam = Camera.main;

            selectionBoxGraphic.GetComponent<Image>().color = selectionBoxColor;

            startDragPosition = Vector2.zero;
            endDragPosition = Vector2.zero;
            DrawDragVisuals();
        }
        private void Start()
        {
            InitializeSelectionManager();
        }
        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            //drag selection, not really needed for survival chaos
            //but would be useful when making new game types
            //need to finish the drag raycast logic
            /*//when clicked
            if (Input.GetMouseButtonDown(0))
            {
                startDragPosition = Input.mousePosition;
            }

            //when dragging
            if(Input.GetMouseButton(0))
            {
                endDragPosition = Input.mousePosition;

                DrawDragVisuals();
                DrawDragSelection();
            }

            //when release click
            if (Input.GetMouseButtonUp(0))
            {
                DragSelectObjects();

                startDragPosition = Vector2.zero;
                endDragPosition = Vector2.zero;

                DrawDragVisuals();
            }*/

            //regular & multi selection
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                ClearSelection();
                TrySelection();
            }
            else
            {
                TryShiftSelection();
            }
        }

        private void TrySelection()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, selectableMask))
            {
                ClearSelection();
                return;
            }

            var selectable = hitInfo.transform.GetComponent<Selectable>();

            if (selectable == null) return;

            SelectObject(selectable);
        }
        private void TryShiftSelection()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, selectableMask)) return;

            var selectable = hitInfo.transform.GetComponent<Selectable>();

            if (selectable == null) return;

            ShiftSelectObject(selectable);
        }
        private void SelectObject(Selectable selectable)
        {
            //set newly selected object cache
            CurrentlySelectedObjects.Add(selectable);
            selectable.selected = true;

            OnObjectSelected?.Invoke(selectable);

            //print($"Selecting {CurrentlySelectedObject}");
        }
        public void ShiftSelectObject(Selectable selectable)
        {
            if (!CurrentlySelectedObjects.Contains(selectable))
            {
                SelectObject(selectable);
            }
        }

        public void ClickSelect(Selectable selectable)
        {
            //DeselectAll();
            CurrentlySelectedObjects.Add(selectable);

            OnObjectSelected?.Invoke(selectable);

            print(selectable);
        }
        public void DragSelect(Selectable selectable)
        {
            if (CurrentlySelectedObjects.Contains(selectable))
            {
                CurrentlySelectedObjects.Add(selectable);
                OnObjectSelected?.Invoke(selectable);
            }
        }
        public void DragSelectObjects()
        {
            foreach (var selectable in CurrentlySelectedObjects)
            {
                if (selectionBox.Contains(cam.WorldToScreenPoint(selectable.transform.position)))
                {
                    DragSelect(selectable);
                }
            }
        }
        private void DrawDragVisuals()
        {
            var boxStart = startDragPosition;
            var boxEnd = endDragPosition;

            var boxCenter = (boxStart + boxEnd) / 2;
            selectionBoxGraphic.position = boxCenter;

            var boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

            selectionBoxGraphic.sizeDelta = boxSize;
        }
        private void DrawDragSelection()
        {
            if(Input.mousePosition.x < startDragPosition.x)
            {
                selectionBox.xMin = Input.mousePosition.x;
                selectionBox.xMax = startDragPosition.x;
            }
            else
            {
                selectionBox.xMin = startDragPosition.x;
                selectionBox.xMax = Input.mousePosition.x;
            }

            if (Input.mousePosition.y < startDragPosition.y)
            {
                selectionBox.yMin = Input.mousePosition.y;
                selectionBox.yMax = startDragPosition.y;
            }
            else
            {
                selectionBox.yMin = startDragPosition.y;
                selectionBox.yMax = Input.mousePosition.y;
            }
        }
        private void ClearSelection()
        {
            if (CurrentlySelectedObjects.Count == 0) return;

            //LastSelectedObject = CurrentlySelectedObjects[CurrentlySelectedObjects.Count];

            foreach (var selectable in CurrentlySelectedObjects)
            {
                selectable.selected = false;

                OnObjectDeselected?.Invoke(selectable);

                //Debug.Log("Deselecting | " + CurrentlySelectedObject);
            }

            CurrentlySelectedObjects.Clear();
        }
    }
}