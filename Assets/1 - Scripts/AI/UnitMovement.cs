using Mirror;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class UnitMovement : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnUnitMoveSpeedChanged))]
        public float unitMovementSpeed;
        public float GetUnitMovementSpeed() => unitMovementSpeed;

        [SyncVar(hook = nameof(OnUnitDestinationChanged))]
        public Vector3 currentDestination;
        public Vector3 GetUnitCurrentDestination() => currentDestination;

        Unit unit;


        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        private void OnEnable()
        {
            unit.OnTarget += OnTargetGet;
            unit.OnTargetClear += OnTargetClear;
        }

        private void OnDisable()
        {
            unit.OnTarget -= OnTargetGet;
            unit.OnTargetClear -= OnTargetClear;
        }

        private void Update()
        {
            var target = unit.GetTarget();
            var attackRange = unit.GetAttackRange();
            var agent = unit.GetAgent();

            //always move agent towards destination unless the target list is empty
            //if (unit.GetTargeter().possibleTargetsInRange.Count == 0) SendToMoveDestination(unitDestination);

            if(unit.IsDead())
            {
                StopAgentMovement(true);
                return;
            }

            if (target == null)
            {
                StopAgentMovement(false);
                unit.inRange = false;

                //waypoints should be removed by the waypoint itself once the unit reaches,
                //then updates its currentDestination with the new waypoint[0]
                if(unit.waypoints.Count != 0) SendToMoveDestination(unit.waypoints[0]);
                return;
            }
            
            var distanceBetweenTarget = (target.transform.position - transform.position).sqrMagnitude;
            unit.inRange = distanceBetweenTarget < attackRange * attackRange;

            StopAgentMovement(unit.inRange);

            /*if (target != null)
            {
                if ((target.transform.position - transform.position).sqrMagnitude >
                    attackRange * attackRange)
                {
                    agent.destination = target.transform.position;
                }

                return;
            }

            if (agent.reachedDestination)
            {
                print("AgentReset");
                RpcResetAgentDestination();
            }*/
        }
        private void ApplyExtraTurnRotation()
        {
            if (unit.GetTarget() == null) return;

            Vector3 targetDirection = unit.GetTarget().transform.position - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Time.deltaTime * 15, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        /// <summary>
        /// called locally by syncvar hook
        /// after the destination is set by server,
        /// the hook passes the newly updated destination to the client,
        /// client then runs the movement and calculations of path locally
        /// </summary>
        /// <param name="destination"></param>
        public void SendToMoveDestination(Vector3 destination)
        {
            //might be stopped after getting in range of target
            StopAgentMovement(false);

            unit.GetAgent().destination = destination;
        }

        /// <summary>
        /// stops agent from moving
        /// does not reset path
        /// </summary>
        public void StopMovement()
        {
            unit.GetAgent().isStopped = true;
        }
        public void StopAgentMovement(bool state)
        {
            unit.GetAgent().isStopped = state;

            ApplyExtraTurnRotation();
        }
        [Server]
        public void ServerSetUnitMovementSpeed(float movementSpeed)
        {
            unitMovementSpeed = movementSpeed;
        }
        [Server]
        public void ServerSetUnitDestination(Vector3 destination)
        {
            currentDestination = destination;
        }

        private void OnTargetGet(Selectable selectable)
        {
            //if has target, set new destination position to it
            SendToMoveDestination(selectable.transform.position);
        }

        private void OnTargetClear()
        {
            Debug.Log("Reseting back to original waypoint!", this);
        }

        [Server]
        private void ServerGetAllWaypoints()
        {
            var waypoints = unit.owner.GetComponent<Barracks>().waypointInfo.barracksWaypoints;

            foreach (var waypoint in waypoints)
            {
                ServerAddWaypointToList(waypoint.position);
            }
        }
        [Server]
        public void ServerAddWaypointToList(Vector3 waypoint)
        {
            unit.waypoints.Add(waypoint);
        }

        IEnumerator WaitUntilBarracks_GetAllWaypoints()
        {
            yield return new WaitUntil(() => unit.owner);

            ServerGetAllWaypoints();
        }

        public override void OnStartServer()
        {
            //all the logic that requires barracks to be set
            StartCoroutine(WaitUntilBarracks_GetAllWaypoints());
        }
        #region Hooks
        private void OnUnitMoveSpeedChanged(float oldSpeed, float newSpeed)
        {
            
        }
        private void OnUnitCanMoveUpdated(bool oldCanMove, bool newCanMove)
        {
            unit.GetAgent().isStopped = newCanMove;
        }
        private void OnUnitDestinationChanged(Vector3 oldDestination, Vector3 newDestination)
        {
            //SendToMoveDestination(newDestination);
        }

        private void OnAgroUpdated(bool oldAgro, bool newAgro)
        {
            Debug.Log($"Agro updated for {name}: {newAgro}", this);
            //print($"Set New Target for {name}: {newTarget}");
        }
        #endregion
    }
}
