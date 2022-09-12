using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class CombatController : NetworkBehaviour
    {
        public event Action OnAttack;
        //should be 0 at start so he attack as soon as inRange
        float attackTimer;

/*        private void Update()
        {
            var target = GetTargeter().GetTarget();

            if (target == null || !GetTargeter().inRange) return;

            TryAttack(target);
        }


        public void TryAttack(Selectable target)
        {
            var targetUnit = target.GetComponent<Unit>();

            if (targetUnit.IsDead()) return;

            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                OnAttack?.Invoke();

                targetUnit.CmdApplyDamage(GetAttack());

                attackTimer = GetAttackRate();
            }
        }
*/    }
}