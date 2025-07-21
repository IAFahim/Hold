using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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

    [HideInInspector] public CharacterState LastAnimationCharacterState;
}

public static class PlatformerCharacterAnimationHandler
{
    public static void UpdateAnimation(
        Animator animator,
        int clipIndexParameterHash,
        float stopMotionFactor,
        ref PlatformerCharacterAnimation characterAnimation,
        in KinematicCharacterBody characterBody,
        in PlatformerCharacterComponent characterComponent,
        in PlatformerCharacterStateMachine characterStateMachine,
        in PlatformerCharacterControl characterControl,
        in LocalTransform localTransform)
    {
        var velocityMagnitude = math.length(characterBody.RelativeVelocity);
        float speed = 1;
        switch (characterStateMachine.CurrentState)
        {
            case CharacterState.GroundMove:
            {
                if (math.length(characterControl.MoveVector) < 0.01f)
                {
                    speed = 1f;
                    animator.SetInteger(clipIndexParameterHash, characterAnimation.IdleClip);
                }
                else
                {
                    if (characterComponent.IsSprinting)
                    {
                        var velocityRatio = velocityMagnitude / characterComponent.GroundSprintMaxSpeed;
                        speed = velocityRatio;
                        animator.SetInteger(clipIndexParameterHash, characterAnimation.SprintClip);
                    }
                    else
                    {
                        var velocityRatio = velocityMagnitude / characterComponent.GroundRunMaxSpeed;
                        speed = velocityRatio;
                        animator.SetInteger(clipIndexParameterHash, characterAnimation.RunClip);
                    }
                }
            }
                break;
            case CharacterState.Crouched:
            {
                if (math.length(characterControl.MoveVector) < 0.01f)
                {
                    speed = 1f;
                    animator.SetInteger(clipIndexParameterHash, characterAnimation.CrouchIdleClip);
                }
                else
                {
                    var velocityRatio = velocityMagnitude / characterComponent.CrouchedMaxSpeed;
                    speed = velocityRatio;
                    animator.SetInteger(clipIndexParameterHash, characterAnimation.CrouchMoveClip);
                }
            }
                break;
            case CharacterState.AirMove:
            {
                speed = 1f;
                animator.SetInteger(clipIndexParameterHash, characterAnimation.InAirClip);
            }
                break;
            case CharacterState.Dashing:
            {
                speed = 1f;
                animator.SetInteger(clipIndexParameterHash, characterAnimation.DashClip);
            }
                break;
            case CharacterState.WallRun:
            {
                var wallIsOnTheLeft = math.dot(MathUtilities.GetRightFromRotation(localTransform.Rotation),
                    characterComponent.LastKnownWallNormal) > 0f;
                speed = 1f;
                animator.SetInteger(clipIndexParameterHash,
                    wallIsOnTheLeft ? characterAnimation.WallRunLeftClip : characterAnimation.WallRunRightClip);
            }
                break;
            case CharacterState.RopeSwing:
            {
                speed = 1f;
                animator.SetInteger(clipIndexParameterHash, characterAnimation.RopeHangClip);
            }
                break;
            case CharacterState.Climbing:
            {
                var velocityRatio = velocityMagnitude / characterComponent.ClimbingSpeed;
                speed = velocityRatio;
                animator.SetInteger(clipIndexParameterHash, characterAnimation.ClimbingMoveClip);
            }
                break;
            case CharacterState.LedgeGrab:
            {
                var velocityRatio = velocityMagnitude / characterComponent.LedgeMoveSpeed;
                speed = velocityRatio;
                animator.SetInteger(clipIndexParameterHash, characterAnimation.LedgeGrabMoveClip);
            }
                break;
            case CharacterState.LedgeStandingUp:
            {
                speed = 1f;
                //animator.SetInteger(ClipIndexParameterHash, characterAnimation.LedgeStandUpClip);
            }
                break;
            case CharacterState.Swimming:
            {
                var velocityRatio = velocityMagnitude / characterComponent.SwimmingMaxSpeed;
                if (velocityRatio < 0.1f)
                {
                    speed = 1f;
                    animator.SetInteger(clipIndexParameterHash, characterAnimation.SwimmingIdleClip);
                }
                else
                {
                    speed = velocityRatio;
                    animator.SetInteger(clipIndexParameterHash, characterAnimation.SwimmingMoveClip);
                }
            }
                break;
            case CharacterState.Rolling:
            case CharacterState.FlyingNoCollisions:
            {
                speed = 1f;
                animator.SetInteger(clipIndexParameterHash, characterAnimation.IdleClip);
            }
                break;
        }

        var stateUnChanged = characterStateMachine.CurrentState == characterAnimation.LastAnimationCharacterState;
        if (stateUnChanged) animator.speed = speed * stopMotionFactor;
        else animator.speed = speed;
        characterAnimation.LastAnimationCharacterState = characterStateMachine.CurrentState;
    }
}