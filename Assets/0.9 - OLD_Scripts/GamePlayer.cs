using Mirror;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class GamePlayer : NetworkBehaviour
    {
        [TargetRpc]
        public void TargetSetPlayerReference(NetworkPlayerOLD roomPlayer)
        {
/*            roomPlayer.gamePlayerRef = this;
            print($"Setting {roomPlayer.gamePlayerRef}");
*/
        }

    }
}