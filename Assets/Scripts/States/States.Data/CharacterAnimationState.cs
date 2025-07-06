using States.States.Data.enums;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace States.States.Data
{
    public struct CharacterAnimationState : IComponentData
    {
        public ECharacterState Previous;
        public ECharacterState Current;

        public void Init()
        {
            
        }


        // public (float speed, int clipIndex) Update(Animator animator)
        // {
        //     float velocityMagnitude = 0;
        //     switch (Current)
        //     {
        //         case ECharacterState.GroundMove:
        //         {
        //             if (math.length(characterControl.MoveVector) < 0.01f)
        //             {
        //                 animator.speed = 1f;
        //                 animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.IdleClip);
        //             }
        //             else
        //             {
        //                 if (characterComponent.IsSprinting)
        //                 {
        //                     float velocityRatio = velocityMagnitude / characterComponent.GroundSprintMaxSpeed;
        //                     animator.speed = velocityRatio;
        //                     animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                         characterAnimation.SprintClip);
        //                 }
        //                 else
        //                 {
        //                     float velocityRatio = velocityMagnitude / characterComponent.GroundRunMaxSpeed;
        //                     animator.speed = velocityRatio;
        //                     animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.RunClip);
        //                 }
        //             }
        //         }
        //             break;
        //         case ECharacterState.Crouched:
        //         {
        //             if (math.length(characterControl.MoveVector) < 0.01f)
        //             {
        //                 animator.speed = 1f;
        //                 animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                     characterAnimation.CrouchIdleClip);
        //             }
        //             else
        //             {
        //                 float velocityRatio = velocityMagnitude / characterComponent.CrouchedMaxSpeed;
        //                 animator.speed = velocityRatio;
        //                 animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                     characterAnimation.CrouchMoveClip);
        //             }
        //         }
        //             break;
        //         case ECharacterState.AirMove:
        //         {
        //             animator.speed = 1f;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.InAirClip);
        //         }
        //             break;
        //         case ECharacterState.Dashing:
        //         {
        //             animator.speed = 1f;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.DashClip);
        //         }
        //             break;
        //         case ECharacterState.WallRun:
        //         {
        //             bool wallIsOnTheLeft = math.dot(MathUtilities.GetRightFromRotation(localTransform.Rotation),
        //                 characterComponent.LastKnownWallNormal) > 0f;
        //             animator.speed = 1f;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                 wallIsOnTheLeft ? characterAnimation.WallRunLeftClip : characterAnimation.WallRunRightClip);
        //         }
        //             break;
        //         case ECharacterState.RopeSwing:
        //         {
        //             animator.speed = 1f;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.RopeHangClip);
        //         }
        //             break;
        //         case ECharacterState.Climbing:
        //         {
        //             float velocityRatio = velocityMagnitude / characterComponent.ClimbingSpeed;
        //             animator.speed = velocityRatio;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.ClimbingMoveClip);
        //         }
        //             break;
        //         case ECharacterState.LedgeGrab:
        //         {
        //             float velocityRatio = velocityMagnitude / characterComponent.LedgeMoveSpeed;
        //             animator.speed = velocityRatio;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                 characterAnimation.LedgeGrabMoveClip);
        //         }
        //             break;
        //         case ECharacterState.LedgeStandingUp:
        //         {
        //             animator.speed = 1f;
        //             //animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.LedgeStandUpClip);
        //         }
        //             break;
        //         case ECharacterState.Swimming:
        //         {
        //             float velocityRatio = velocityMagnitude / characterComponent.SwimmingMaxSpeed;
        //             if (velocityRatio < 0.1f)
        //             {
        //                 animator.speed = 1f;
        //                 animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                     characterAnimation.SwimmingIdleClip);
        //             }
        //             else
        //             {
        //                 animator.speed = velocityRatio;
        //                 animator.SetInteger(characterAnimation.ClipIndexParameterHash,
        //                     characterAnimation.SwimmingMoveClip);
        //             }
        //         }
        //             break;
        //         case ECharacterState.Rolling:
        //         case ECharacterState.FlyingNoCollisions:
        //         {
        //             animator.speed = 1f;
        //             animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.IdleClip);
        //         }
        //             break;
        //     }
        // }
    }
}