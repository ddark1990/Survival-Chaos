using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SurvivalChaos
{
    public class OutlineRelay : MonoBehaviour
    {
        public Color OutlineColor {
            get { return outlineColor; }
            set { outlineColor = value; }
        }

        public float OutlineWidth {
            get { return outlineWidth; }
            set { outlineWidth = value; }
        }

        private Color outlineColor;
        private float outlineWidth = 2;
    
        public Renderer[] meshRenderers;
        [HideInInspector] public Material[] cachedMaterials;
        [HideInInspector] public Material[] outlineMaterials;
        private Texture _cachedTexture;
        private Color _cachedColor;

        private Material _outlineMaskMaterial;
        private Material _outlineMaterial;
    
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int Width = Shader.PropertyToID("_Border");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        private SelectionManager _selectionManager;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private OutlineRelay _outline;
        private Color _tempColor;

        private void Awake()
        {
            meshRenderers = GetComponentsInChildren<Renderer>();
        }
    
        private void Start()
        {
            _selectionManager = SelectionManager.Instance;

            CacheMaterialData(); 
            //CreateOutlineMaterials();
        }

        private void Update()
        {        
            //UpdateMaterials();
        }

        /*public void CreateOutlineMaterials() 
        {
            _outlineMaskMaterial = Instantiate(_selectionManager.outlineMaskMaterial);
            _outlineMaterial = Instantiate(_selectionManager.outlineMaterial);

            SetMaterialData();
            
            if(outlineMaterials.Length == 0)
                outlineMaterials = new Material[2];
            
            outlineMaterials[0] = _outlineMaskMaterial;
            outlineMaterials[1] = _outlineMaterial;
            
            *//*
            var materials = meshRenderer.sharedMaterials.ToList();

            materials.Add(_outlineMaskMaterial);
            materials.Add(_outlineMaterial);

            meshRenderer.materials = materials.ToArray();
        *//*
        }*/
    
        public void CacheMaterialData()
        {
            cachedMaterials = new Material[meshRenderers[0].sharedMaterials.Length];

            var materials = meshRenderers[0].materials;
            
            cachedMaterials = materials; //cache original materials

            foreach (var material in cachedMaterials) //cache each of the materials main textures
            {
                _cachedTexture = material.mainTexture;
                _cachedColor = material.color;
            }
        }
        
        private void SetMaterialData()
        {
            _outlineMaskMaterial.SetTexture(BaseMap, _cachedTexture);
            _outlineMaskMaterial.SetColor(BaseColor, _cachedColor);

            _outlineMaskMaterial.name = "OutlineMask (Instance)";
            _outlineMaterial.name = "OutlineFill (Instance)";
        }
        
        private void UpdateMaterials()
        {
            _outlineMaterial.SetColor(Color, outlineColor);
            _outlineMaterial.SetFloat(Width, outlineWidth);
        }
/*        protected void OutlineHighlight()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            _outline.meshRenderers[0].sharedMaterials = _outline.outlineMaterials;
            _outline.OutlineColor = Color.Lerp(_outline.OutlineColor, _tempColor, _selectionManager.fadeSpeed * Time.deltaTime);

            if (selected)
            {
                _tempColor = _selectionManager.selectedColor;

                return;
            }

            if (SelectionManager.Instance.HoveringObject != null && SelectionManager.Instance.HoveringObject.Equals(this))
            {
                _tempColor = _selectionManager.hoverOverColor;

                return;
            }

            if (SelectionManager.Instance.CurrentlySelectedObject == this && SelectionManager.Instance.HoveringObject == this) return;

            _tempColor = Color.clear;

            if (_outline.OutlineColor.a <= 0.5f)
            {
                _outline.meshRenderers[0].sharedMaterials = _outline.cachedMaterials;
            }
        }
*/
        private void OnMouseEnter()
        {
            
        }

        private void OnMouseExit()
        {
            
        }
    }
}
