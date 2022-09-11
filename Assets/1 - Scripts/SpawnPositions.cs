using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SurvivalChaos
{
    public class SpawnPositions : NetworkBehaviour
    {
        public SyncList<Transform> startingPositions = new SyncList<Transform>();


    }
}
