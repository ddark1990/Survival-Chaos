using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class UI_SelectionRender : MonoBehaviour
    {
        public static UI_SelectionRender instance;

        public CinemachineVirtualCamera unitsModelRenderCamera;
        public CinemachineVirtualCamera buildingsModelRenderCamera;

        public GameObject unitsModelUIRender;
        public GameObject buildingsModelUIRender;

        List<Renderer> buildingRenderers;
        //List<MeshFilter> buildingMeshFilters;
        //List<Material> buildingMaterials;


        private void Awake()
        {
            instance = this;

            //buildingRenderers = GetRenderers(buildingsModelUIRender);
            //buildingMeshFilters = GetFilters(buildingsModelUIRender);
            //buildingMaterials = GetMaterials(buildingRenderers);
        }

        private void OnEnable()
        {
            SelectionManager.OnObjectSelected += UpdateRenderUIModel;
            //SelectionManager.OnObjectDeselected += DeactivateUI;
        }

        private void OnDisable()
        {
            SelectionManager.OnObjectSelected -= UpdateRenderUIModel;
        }
        private void UpdateRenderUIModel(Selectable selectable)
        {
            if(selectable.GetComponent<Unit>())
            {
                foreach (Transform child in unitsModelUIRender.transform)
                {
                    Destroy(child.gameObject);
                }

                Instantiate(selectable.scriptableObjectData.modelUIRender, unitsModelUIRender.transform);
            }
            else
            {
                foreach (Transform child in buildingsModelUIRender.transform)
                {
                    Destroy(child.gameObject);
                }

                Instantiate(selectable.scriptableObjectData.modelUIRender, buildingsModelUIRender.transform);

                /*                var buildingRenderers = GetRenderers(buildingsModelUIRender);
                                var buildingMeshFilters = GetFilters(buildingsModelUIRender);

                                var buildingRenderer = buildingRenderers[0];
                                var buildingFilter = buildingMeshFilters[0];

                                if(buildingFilter == null)
                                {
                                    buildingFilter.mesh = selectable.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                                    return;
                                }

                                buildingFilter.mesh = selectable.GetComponentInChildren<MeshFilter>().mesh;
                                buildingRenderer.material = selectable.GetComponentInChildren<Renderer>().material;
                */
            }
        }

/*        private List<Renderer> GetRenderers(GameObject renderObject)
        {
            List<Renderer> renderers = new List<Renderer>();

            if(renderObject.GetComponent<Renderer>()) renderers.Add(renderObject.GetComponent<Renderer>());

            foreach (Transform child in renderObject.transform)
            {
                if (child.GetComponent<SpriteRenderer>()) continue;

                if (child.gameObject.activeInHierarchy && child.GetComponent<Renderer>())
                {
                    renderers.Add(child.GetComponent<Renderer>());
                }
            }

            return renderers;
        }
        private List<MeshFilter> GetFilters(GameObject renderObject)
        {
            List<MeshFilter> filters = new List<MeshFilter>();

            if (renderObject.GetComponent<Renderer>()) filters.Add(renderObject.GetComponent<MeshFilter>());

            foreach (Transform child in renderObject.transform)
            {
                if (child.GetComponent<SpriteRenderer>()) continue;

                if (child.gameObject.activeInHierarchy && child.GetComponent<MeshFilter>())
                {
                    filters.Add(child.GetComponent<MeshFilter>());
                }
            }

            return filters;
        }
*//*        private List<Material> GetMaterials(List<Renderer> renderers)
        {
            List<Material> materials = new List<Material>();

            foreach (Renderer renderer in renderers)
            {
                materials.Add(renderer.material);
            }

            return materials;
        }
*/
    }
}