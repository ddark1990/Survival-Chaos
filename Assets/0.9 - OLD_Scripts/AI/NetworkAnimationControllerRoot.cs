using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace SurvivalChaos
{
    public class NetworkAnimationControllerRoot : NetworkBehaviour
    {
        [SerializeField] private float moveSpeedMultiplier = 1;
        [SerializeField] private float animSpeedMultiplier = 1;
        [SerializeField] private float turnSpeedMultiplier = 360;
        [SerializeField] private float stationaryTurnSpeed = 180;

        [SyncVar]
        public float forwardAmount;
        [SyncVar]
        [SerializeField] private float turnAmount;
        [SyncVar]
        [SerializeField] private bool moveFaster;

        [HideInInspector] public int hash;
        [HideInInspector] public Animator lastAnimatorCache; 
        [HideInInspector] public Dictionary<string,int> animatorParamCache 
            = new Dictionary<string,int>( );

        Unit unit;
        Animator animator;
        private int Forward = Animator.StringToHash("Forward");
        private int Turn = Animator.StringToHash("Turn");
        private int MoveFaster = Animator.StringToHash("MoveFaster");

        private void Awake()
        {
            unit = GetComponent<Unit>();
            animator = GetComponent<Animator>();
        }

        private void Update() 
        {
            UpdateAnimator();
            ApplyExtraTurnRotation();

            Move(unit.GetAgent().remainingDistance > unit.GetAgent().endReachedDistance
                ? unit.GetAgent().desiredVelocity
                : Vector3.zero);

            if (!hasAuthority)return;
            

        }
    
        private void HandleNetworkVelocity(Vector3 oldV, Vector3 newV)
        {
            //print($"Desired Velocity {newV}");
        }

        [Command]
        private void SetForwardAmount(float forwardAmount)
        {
            this.forwardAmount = forwardAmount;
        }

        [Command]
        private void SetTurnAmount(float turnAmount)
        {
            this.turnAmount = turnAmount;
        }

        public void Move(Vector3 move)
        {
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);

            SetForwardAmount(move.z);
            SetTurnAmount(Mathf.Atan2(move.x, move.z));
/*            forwardAmount = move.z;
            turnAmount = Mathf.Atan2(move.x, move.z);
*/
            unit.GetAgent().MovementUpdate(Time.deltaTime, out move, out var nextRotation);
            animator.applyRootMotion = true;
        }

        Vector3 deltaPosition;
        Vector3 velocity;
        Vector3 v;

        public void SetRootMotion() 
        {
            deltaPosition = animator.deltaPosition;
            velocity = deltaPosition * moveSpeedMultiplier / Time.deltaTime;
            
            v = (deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = velocity.y;
            velocity = v;
            unit.GetRigidbody().velocity = velocity;

        }

        public void OnAnimatorMove()
        {
            SetRootMotion();
        }
        
        private void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            var turnSpeed = Mathf.Lerp(stationaryTurnSpeed, turnSpeedMultiplier, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        private void UpdateAnimator()
        {
            animator.SetFloat("Forward", forwardAmount);
            animator.SetFloat("Turn", turnAmount);

            // if( TryGetAnimatorParam(  animator, lastAnimatorCache, animatorParamCache, "Forward", out hash ) ) 
            // {
            //     animator.SetFloat(Forward, forwardAmount);
            // }
            // if( TryGetAnimatorParam(  animator, lastAnimatorCache, animatorParamCache, "Turn", out hash ) ) 
            // {
            //     animator.SetFloat(Turn, turnAmount);
            // }
            // if( TryGetAnimatorParam(  animator, lastAnimatorCache, animatorParamCache, "MoveFaster", out hash ) ) 
            // {
            //     animator.SetBool(MoveFaster, moveFaster);
            // }
            
            if (unit.GetRigidbody().velocity.magnitude > 0)
            {
                animator.speed = animSpeedMultiplier;
            }            
        }

         //caches and resolves the params with no GC allocation per param
        public static bool TryGetAnimatorParam( Animator animator,Animator lastAnimator, Dictionary<string,int> animatorParam, string paramName, out int hash )
        {
            // Rebuild cache
            if( (lastAnimator == null || lastAnimator != animator) && animator != null ) 
            {
                lastAnimator = animator;
                animatorParam.Clear( );
                foreach( var param in animator.parameters )
                {
                    // could use param.nameHash property but this is clearer
                    var paramHash = Animator.StringToHash( param.name ); 
                    animatorParam.Add( param.name, paramHash );
                }
            }

            if( animatorParam != null && animatorParam.TryGetValue( paramName, out hash ) )
            {
                return true;
            }
            else
            {
                hash = 0;
                return false;
            }
        }
    }
}