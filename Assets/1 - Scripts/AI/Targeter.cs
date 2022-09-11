using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace SurvivalChaos
{
    public class Targeter : NetworkBehaviour
    {
        [SerializeField] LayerMask agroMask;
        [SerializeField] Collider[] hitColliders;
        public bool inRange;

        [SerializeField] private Selectable target;
        public Selectable GetTarget() => target;

        //TODO:network this, and show it de wey
        float agroRadius = 5;

        private void Update()
        {
            if(target != null && target.IsDead())
            {
                ServerClearTarget();
            }

            //breaks everything if commented out
            //if (target != null || IsDead()) return;


            //non allocating version to spherecast
            //var ray = new Ray(transform.position, transform.forward);
            //var hits = new RaycastHit[1];
            //var numberOfHits = Physics.SphereCastNonAlloc(ray, agroRadius, hits, 0, agroMask);

            //pre sure has some garbage allocation
            hitColliders = Physics.OverlapSphere(transform.position, agroRadius, agroMask);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                var hit = hitColliders[i];

                //targets first unit that it sees, might cuz issues
                var selecatable = hit.GetComponent<Selectable>();
                var hasAuthority = selecatable.hasAuthority;

                //dont target if we own the object
                if (hasAuthority) continue;

                TryTarget(selecatable);
            }
        }
        public void TryTarget(Selectable selectable)
        {
            CmdSetTarget(selectable.gameObject);
        }

        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject) return;

            if (!targetGameObject.TryGetComponent<Selectable>(out Selectable newTarget)) { return; }

            //set target on own client
            target = newTarget;
            //and on and other players clients
            RpcSetTarget(targetGameObject);

            //add self to the targets attackers list
            RpcAddToAttackingList(newTarget);

            //*MOVE, USE EVENT TO EXECUTE
            //move towards target if unit
            //unit.GetUnitMovement().ServerSetUnitDestination(target.transform.position);
        }

        [ClientRpc]
        private void RpcAddToAttackingList(Selectable newTarget)
        {
            if (newTarget == null) return;

            newTarget.attackingObjects.Add(newTarget);
        }

        [ClientRpc]
        private void RpcSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject) return;

            if (!targetGameObject.TryGetComponent<Selectable>(out Selectable newTarget)) { return; }

            target = newTarget;
        }

        [Server]
        public void ServerClearTarget()
        {
            target = null;
            RpcClearTarget();
        }

        [ClientRpc]
        private void RpcClearTarget()
        {
            target = null;
        }

        /*        private void SetTarget(GameObject target)
                {
                    this.target = target;
                }
        */
        /*        [Command]
                public void CmdSetTarget(GameObject target)
                {
                    RpcSetTarget(target);
                }

                [ClientRpc]
                public void RpcSetTarget(GameObject target)
                {
                    SetTarget(target);
                    print($"RpcSetTarget: {target}");
                }

                [TargetRpc]
                public void TargetSetTarget(NetworkConnection conn, GameObject target)
                {
                    SetTarget(target);
                    print($"TargetSetTarget: {conn} : {target}");
                }

                [Server]
                public void ClearTarget()
                {
                    target = null;
                }

                private void OnTargetSet(GameObject oldTarget, GameObject newTarget)
                {
                    Debug.Log($"Set New Target for {name}: {newTarget}", this);
                    //print($"Set New Target for {name}: {newTarget}");
                }
        */
        #region Debug

        //TODO: create line render debug for in game options
        /*private void OnDrawGizmosSelected()
        {
            //agro radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, agroRadius);

            //attack range
            Gizmos.color = Color.green;
            //Gizmos.DrawWireSphere(transform.position, GetAttackRange());
        }*/
        #endregion

    }
}
