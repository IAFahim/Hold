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
    public const int IdleClip = 0;
    public const int RunClip = 1;
    public const int SprintClip = 2;
    public const int InAirClip = 3;
    public const int LedgeGrabMoveClip = 4;
    public const int LedgeStandUpClip =5;
    public const int WallRunLeftClip=6;
    public const int WallRunRightClip=7;
    public const int CrouchIdleClip=8;
    public const int CrouchMoveClip=9;
    public const int ClimbingMoveClip=10;
    public const int SwimmingIdleClip=11;
    public const int SwimmingMoveClip=12;
    public const int DashClip=13;
    public const int RopeHangClip=14;
    public const int SlidingClip=15;
    public const int HitClip = 14;

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
            clipId = PlatformerCharacterAnimation.HitClip;
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
                    clipId = PlatformerCharacterAnimation.IdleClip;
                }
                else
                {
                    if (characterComponent.IsSprinting)
                    {
                        var velocityRatio = velocityMagnitude / characterComponent.GroundSprintMaxSpeed;
                        speed = velocityRatio;
                        clipId = PlatformerCharacterAnimation.SprintClip;
                    }
                    else
                    {
                        var velocityRatio = velocityMagnitude / characterComponent.GroundRunMaxSpeed;
                        speed = velocityRatio;
                        clipId = PlatformerCharacterAnimation.RunClip;
                    }
                }
            }
                break;
            case CharacterState.Crouched:
            {
                if (math.length(characterControl.MoveVector) < 0.01f)
                {
                    speed = 1f;
                    clipId = PlatformerCharacterAnimation.CrouchIdleClip;
                }
                else
                {
                    var velocityRatio = velocityMagnitude / characterComponent.CrouchedMaxSpeed;
                    speed = velocityRatio;
                    clipId = PlatformerCharacterAnimation.CrouchMoveClip;
                }
            }
                break;
            case CharacterState.AirMove:
            {
                speed = 1f;
                clipId = PlatformerCharacterAnimation.InAirClip;
            }
                break;
            case CharacterState.Dashing:
            {
                speed = 1f;
                clipId = PlatformerCharacterAnimation.DashClip;
            }
                break;
            case CharacterState.WallRun:
            {
                var wallIsOnTheLeft = math.dot(MathUtilities.GetRightFromRotation(localTransform.Rotation),
                    characterComponent.LastKnownWallNormal) > 0f;
                speed = 1f;
                clipId =
                    wallIsOnTheLeft ? PlatformerCharacterAnimation.WallRunLeftClip : PlatformerCharacterAnimation.WallRunRightClip;
            }
                break;
            case CharacterState.RopeSwing:
            {
                speed = 1f;
                clipId = PlatformerCharacterAnimation.RopeHangClip;
            }
                break;
            case CharacterState.Climbing:
            {
                var velocityRatio = velocityMagnitude / characterComponent.ClimbingSpeed;
                speed = velocityRatio;
                clipId = PlatformerCharacterAnimation.ClimbingMoveClip;
            }
                break;
            case CharacterState.LedgeGrab:
            {
                var velocityRatio = velocityMagnitude / characterComponent.LedgeMoveSpeed;
                speed = velocityRatio;
                clipId = PlatformerCharacterAnimation.LedgeGrabMoveClip;
            }
                break;
            case CharacterState.LedgeStandingUp:
            {
                speed = 1f;
                clipId = PlatformerCharacterAnimation.LedgeStandUpClip;
            }
                break;
            case CharacterState.Swimming:
            {
                var velocityRatio = velocityMagnitude / characterComponent.SwimmingMaxSpeed;
                if (velocityRatio < 0.1f)
                {
                    speed = 1f;
                    clipId = PlatformerCharacterAnimation.SwimmingIdleClip;
                }
                else
                {
                    speed = velocityRatio;
                    clipId = PlatformerCharacterAnimation.SwimmingMoveClip;
                }
            }
                break;
            case CharacterState.Sliding:
            {
                speed = 1f;
                clipId = PlatformerCharacterAnimation.SlidingClip;
            }
                break;
            case CharacterState.Rolling:
            case CharacterState.FlyingNoCollisions:
            {
                speed = 1f;
                clipId = PlatformerCharacterAnimation.IdleClip;
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