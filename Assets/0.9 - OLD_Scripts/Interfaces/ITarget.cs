using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public interface ITarget
    {
        void TryTarget(Selectable selectable);
        void CmdSetTarget(GameObject targetGameObject);
        void RpcAddToAttackingList(Selectable newTarget);
        void RpcSetTarget(GameObject targetGameObject);
        void ServerClearTarget();
        void RpcClearTarget();
    }
}