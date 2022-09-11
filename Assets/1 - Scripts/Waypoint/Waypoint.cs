using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class Waypoint : NetworkBehaviour
    {
        [Server]
        private void OnTriggerEnter(Collider other)
        {
            ServerDeleteWaypoint(other.gameObject);
        }

        [Server]
        private void ServerDeleteWaypoint(GameObject unit)
        {
            var waypoints = unit.GetComponent<Unit>().waypoints;

            foreach (var waypoint in waypoints)
            {
                if(waypoint == transform.position)
                {
                    waypoints.Remove(waypoint);
                }
            }
        }
    }
}
