using System;
using Follows.Follows.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.CharacterController;

[Serializable]
public struct PlatformerCharacterAnimation : IComponentData
{
    [HideInInspector] public int IdleClip;
    [HideInInspector] public int RunClip;
    [HideInInspector] public int SprintClip;
    [HideInInspector] public int InAirClip;
    [HideInInspector] public int LedgeGrabMoveClip;
    [HideInInspector] public int LedgeStandUpClip;
    [HideInInspector] public int WallRunLeftClip;
    [HideInInspector] public int WallRunRightClip;
    [HideInInspector] public int CrouchIdleClip;
    [HideInInspector] public int CrouchMoveClip;
    [HideInInspector] public int ClimbingMoveClip;
    [HideInInspector] public int SwimmingIdleClip;
    [HideInInspector] public int SwimmingMoveClip;
    [HideInInspector] public int DashClip;
    [HideInInspector] public int RopeHangClip;
    [HideInInspector] public int SlidingClip;
    [HideInInspector] public int HitClip;

    [HideInInspector] public CharacterState LastAnimationCharacterState;
}

public static class PlatformerCharacterAnimationHandler
{
    private static readonly int ClipIndex = Animator.StringToHash("ClipIndex");

    public static void UpdateAnimation(
        Animator animator,
        ref PlatformerCharacterAnimation characterAnimation,
        in KinematicCharacterBody characterBody,
        in PlatformerCharacterComponent characterComponent,
        in PlatformerCharacterStateMachine characterStateMachine,
        in PlatformerCharacterControl characterControl,
        in LocalTransform localTransform,
        in FollowEnableComponent followEnableComponent
    )
    {
        float speed = 1;
        var clipId = 0;
        if (followEnableComponent.Reached)
        {
            clipId = characterAnimation.HitClip;
            speed = 1;
            SetAnimationToGameobject(animator, clipId, speed, ref characterAnimation, characterStateMachine);
            return;
        }

        var velocityMagnitude = math.length(characterBody.RelativeVelocity);
        switch (characterStateMachine.CurrentState)
        {
            case CharacterState.GroundMove:
            {
                if (math.length(characterControl.MoveVector) < 0.01f)
                {
                    speed = 1f;
                    clipId = characterAnimation.IdleClip;
                }
                else
                {
                    if (characterComponent.IsSprinting)
                    {
                        var velocityRatio = velocityMagnitude / characterComponent.GroundSprintMaxSpeed;
                        speed = velocityRatio;
                        clipId = characterAnimation.SprintClip;
                    }
                    else
                    {
                        var velocityRatio = velocityMagnitude / characterComponent.GroundRunMaxSpeed;
                        speed = velocityRatio;
                        clipId = characterAnimation.RunClip;
                    }
                }
            }
                break;
            case CharacterState.Crouched:
            {
                if (math.length(characterControl.MoveVector) < 0.01f)
                {
                    speed = 1f;
                    clipId = characterAnimation.CrouchIdleClip;
                }
                else
                {
                    var velocityRatio = velocityMagnitude / characterComponent.CrouchedMaxSpeed;
                    speed = velocityRatio;
                    clipId = characterAnimation.CrouchMoveClip;
                }
            }
                break;
            case CharacterState.AirMove:
            {
                speed = 1f;
                clipId = characterAnimation.InAirClip;
            }
                break;
            case CharacterState.Dashing:
            {
                speed = 1f;
                clipId = characterAnimation.DashClip;
            }
                break;
            case CharacterState.WallRun:
            {
                var wallIsOnTheLeft = math.dot(MathUtilities.GetRightFromRotation(localTransform.Rotation),
                    characterComponent.LastKnownWallNormal) > 0f;
                speed = 1f;
                clipId =
                    wallIsOnTheLeft ? characterAnimation.WallRunLeftClip : characterAnimation.WallRunRightClip;
            }
                break;
            case CharacterState.RopeSwing:
            {
                speed = 1f;
                clipId = characterAnimation.RopeHangClip;
            }
                break;
            case CharacterState.Climbing:
            {
                var velocityRatio = velocityMagnitude / characterComponent.ClimbingSpeed;
                speed = velocityRatio;
                clipId = characterAnimation.ClimbingMoveClip;
            }
                break;
            case CharacterState.LedgeGrab:
            {
                var velocityRatio = velocityMagnitude / characterComponent.LedgeMoveSpeed;
                speed = velocityRatio;
                clipId = characterAnimation.LedgeGrabMoveClip;
            }
                break;
            case CharacterState.LedgeStandingUp:
            {
                speed = 1f;
                // clipId = characterAnimation.LedgeStandUpClip;
            }
                break;
            case CharacterState.Swimming:
            {
                var velocityRatio = velocityMagnitude / characterComponent.SwimmingMaxSpeed;
                if (velocityRatio < 0.1f)
                {
                    speed = 1f;
                    clipId = characterAnimation.SwimmingIdleClip;
                }
                else
                {
                    speed = velocityRatio;
                    clipId = characterAnimation.SwimmingMoveClip;
                }
            }
                break;
            case CharacterState.Sliding:
            {
                speed = 1f;
                clipId = characterAnimation.SlidingClip;
            }
                break;
            case CharacterState.Rolling:
            case CharacterState.FlyingNoCollisions:
            {
                speed = 1f;
                clipId = characterAnimation.IdleClip;
            }
                break;
        }

        SetAnimationToGameobject(animator, clipId, speed, ref characterAnimation, characterStateMachine);
    }

    private static void SetAnimationToGameobject(
        Animator animator, int clipId, float speed,
        ref PlatformerCharacterAnimation characterAnimation,
        PlatformerCharacterStateMachine characterStateMachine
    )
    {
        
        // var stateUnChanged = characterStateMachine.CurrentState == characterAnimation.LastAnimationCharacterState;
        // if (stateUnChanged)
        // {
        //     animator.speed = speed * stopMotionFactor;
        //     return;
        // }
        animator.speed = speed;
        animator.SetInteger(ClipIndex, clipId);
        characterAnimation.LastAnimationCharacterState = characterStateMachine.CurrentState;
    }
}