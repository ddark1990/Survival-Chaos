using Mirror;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class Unit : Selectable
    {
        //ref for death skeleton, turn into array later for 
        public GameObject deathSkeleton;

        [Header("Unit")]
        //public GameObject owningBarracks;

        private AIPath agent;
        public AIPath GetAgent() => agent;
        private Rigidbody rigidBody;
        public Rigidbody GetRigidbody() => rigidBody;
        private Animator animator;
        public Animator GetAnimator() => animator;
        private UnitMovement unitMovement;
        public UnitMovement GetUnitMovement() => unitMovement;
        public List<Renderer> renderers;
        public List<Renderer> GetRenderers() => renderers;

        public readonly SyncList<Vector3> waypoints = new SyncList<Vector3>();

        private void Awake()
        {
            CacheUnitComponents();
        }

        private void OnEnable()
        {
            OnObjectDeath += OnUnitDeath;
            MaterialIndexUpdated += CmdSetUnitMaterials;
        }

        private void OnDisable()
        {
            OnObjectDeath -= OnUnitDeath;
            MaterialIndexUpdated -= CmdSetUnitMaterials;
        }

        //[ClientRpc]
        private void OnUnitDeath(Selectable selectable)
        {
            StartCoroutine(DeathAnimation());
        }

        //make better
        IEnumerator DeathAnimation()
        {
            yield return new WaitForSeconds(1.5f);

            if (deathSkeleton != null)
            {
                deathSkeleton.SetActive(true);
            }

            yield return new WaitForSeconds(2);

            LeanTween.value(gameObject, transform.position, transform.position + new Vector3(0, -1, 0), 0.5f).setOnUpdate((Vector3 val) => {
                transform.position = val;
            });

            if(deathSkeleton != null)
            {
                LeanTween.value(deathSkeleton, deathSkeleton.transform.position, deathSkeleton.transform.position, 0.5f).setOnUpdate((Vector3 val) => {
                    deathSkeleton.transform.position = val;
                });

                yield return new WaitForSeconds(2);

                LeanTween.value(deathSkeleton, deathSkeleton.transform.position, deathSkeleton.transform.position + new Vector3(0, -1, 0), 0.5f).setOnUpdate((Vector3 val) => {
                    deathSkeleton.transform.position = val;
                });
            }

            yield return new WaitForSeconds(1);

            NetworkServer.Destroy(gameObject);
        }

        #region Server


        [Command]
        private void CmdSetUnitMaterials(int newIndex)
        {
            RpcSetUnitMaterials(newIndex);
        }
        
        //color materials should be general for all selectables
        [ClientRpc]
        private void RpcSetUnitMaterials(int newIndex)
        {
            foreach (var renderer in GetRenderers())
            {
                renderer.material = (owner.GetComponent<Barracks>().scriptableObjectData as BarracksScriptableData).unitColorMaterials[newIndex];
            }
        }

/*        [ClientRpc]
        public void RpcSetUnitOwningBarracks(GameObject barracks)
        {
            owningBarracks = barracks;
        }
*/
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
        }
        #endregion

        #region Client

        public void CacheUnitComponents()
        {
            CacheGeneralDataComponents();

            TryGetComponent(out agent);
            TryGetComponent(out rigidBody);
            TryGetComponent(out animator);
            TryGetComponent(out unitMovement);

            foreach (Transform child in transform)
            {
                if (child.GetComponent<SpriteRenderer>()) continue;

                if (child.gameObject.activeInHierarchy && child.GetComponent<Renderer>())
                {
                    renderers.Add(child.GetComponent<Renderer>());
                }
            }
        }


        #endregion

        #region Hooks


        #endregion

    }
}
