using Mirror;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class NetworkAnimationController : NetworkBehaviour
    {
        Unit unit;
        string currentState;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        private void OnEnable()
        {
            unit.OnAttack += AnimationAttackTrigger;
            unit.OnUnitStopAttackAnimation += StopAttackAnimation;
            unit.OnObjectDeath += AnimDeathTrigger;
        }

        private void OnDisable()
        {
            unit.OnAttack -= AnimationAttackTrigger;
            unit.OnUnitStopAttackAnimation -= StopAttackAnimation;
            unit.OnObjectDeath -= AnimDeathTrigger;
        }

        private void Update()
        {
            //if (!hasAuthority) return;

            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            unit.GetAnimator().SetFloat("Speed", unit.GetAgent().desiredVelocity.magnitude);
            unit.GetAnimator().SetBool("InCombat", unit.GetTarget());

            //save for later for utility method that hashes the ints for the animator
            //so no garbage collection is made during animation

            /*if( TryGetAnimatorParam(  animator, lastAnimatorCache, animatorParamCache, "Forward", out hash ) ) 
            {
                animator.SetFloat(Forward, forwardAmount);
            }
            if( TryGetAnimatorParam(  animator, lastAnimatorCache, animatorParamCache, "Turn", out hash ) ) 
            {
                animator.SetFloat(Turn, turnAmount);
            }
            if( TryGetAnimatorParam(  animator, lastAnimatorCache, animatorParamCache, "MoveFaster", out hash ) ) 
            {
                animator.SetBool(MoveFaster, moveFaster);
            }*/
        }

        public void AnimationAttackTrigger()
        {
/*            attackingUnit.GetAnimator().SetInteger("AttackIndex", UnityEngine.Random.Range(0, 2));
            attackingUnit.GetAnimator().SetTrigger("Attack");
*/
            PlayAnimationState("Unit_Attack_1", 0);
        }
        public void StopAttackAnimation(Unit attackingUnit)
        {
            //stop attack animation somehow
            PlayAnimationState("LM_Blend", 0);
        }

        [ClientRpc]
        public void AnimDeathTrigger(Selectable attackingUnit)
        {
            /*attackingUnit.GetComponent<Unit>().GetAnimator().SetInteger("DeathIndex", UnityEngine.Random.Range(0, 2));
            attackingUnit.GetComponent<Unit>().GetAnimator().SetTrigger("Die");*/

            PlayAnimationState("Unit_Death_2", 0);
        }

        private void PlayAnimationState(string newState, int animationLayer)
        {
            //if (currentState == newState) return;

            unit.GetAnimator().Play(newState, animationLayer);
        }

    }
}