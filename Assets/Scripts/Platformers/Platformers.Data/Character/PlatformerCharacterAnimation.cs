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
    // Animation clip indices
    public const int IdleClip = 0;
    public const int RunClip = 1;
    public const int SprintClip = 2;
    public const int InAirClip = 3;
    public const int LedgeGrabMoveClip = 4;
    public const int LedgeStandUpClip = 5;
    public const int WallRunLeftClip = 6;
    public const int WallRunRightClip = 7;
    public const int CrouchIdleClip = 8;
    public const int CrouchMoveClip = 9;
    public const int ClimbingMoveClip = 10;
    public const int SwimmingIdleClip = 11;
    public const int SwimmingMoveClip = 12;
    public const int DashClip = 13;
    public const int RopeHangClip = 14;
    public const int SlidingClip = 15;
    public const int HitClip = 16;

    [HideInInspector] public CharacterState LastAnimationCharacterState;
    [HideInInspector] public float LastLoadRatio;
    
    // Animation modifiers for carrying
    public float breathingIntensityThreshold; // Load ratio where breathing becomes noticeable
    public float heavyBreathingLoadRatio; // Load ratio for heavy breathing animations
}

[BurstCompile]
public struct AnimationData
{
    public int clipIndex;
    public float baseSpeed;
    public float velocityBasedSpeed;
    
    public AnimationData(int clip, float speed = 1f, float velocitySpeed = 0f)
    {
        clipIndex = clip;
        baseSpeed = speed;
        velocityBasedSpeed = velocitySpeed;
    }
}

public static class PlatformerCharacterAnimationHandler
{
    private static readonly int ClipIndex = Animator.StringToHash("ClipIndex");
    private static readonly int LoadRatio = Animator.StringToHash("LoadRatio");
    private static readonly int IsBreathingHeavy = Animator.StringToHash("IsBreathingHeavy");
    private static readonly int CarryingWeight = Animator.StringToHash("CarryingWeight");

    public static void UpdateAnimation(
        Animator animator,
        ref PlatformerCharacterAnimation characterAnimation,
        in KinematicCharacterBody characterBody,
        in PlatformerCharacterComponent characterComponent,
        in PlatformerCharacterStateMachine characterStateMachine,
        in PlatformerCharacterControl characterControl,
        in LocalTransform localTransform,
        in FollowEnableComponent followEnableComponent,
        in CarryingComponent carryingComponent
    )
    {
        // Handle special states first
        if (followEnableComponent.Reached)
        {
            SetAnimation(animator, ref characterAnimation, characterStateMachine, carryingComponent,
                new AnimationData(PlatformerCharacterAnimation.HitClip, 1f));
            return;
        }

        // Calculate base animation data based on character state
        var animationData = GetAnimationDataForState(
            characterStateMachine.CurrentState,
            characterComponent,
            characterControl,
            characterBody,
            localTransform
        );

        // Apply animation with carrying modifiers
        SetAnimation(animator, ref characterAnimation, characterStateMachine, carryingComponent, animationData);
    }

    [BurstCompile]
    private static AnimationData GetAnimationDataForState(
        CharacterState state,
        in PlatformerCharacterComponent characterComponent,
        in PlatformerCharacterControl characterControl,
        in KinematicCharacterBody characterBody,
        in LocalTransform localTransform)
    {
        var velocityMagnitude = math.length(characterBody.RelativeVelocity);
        var moveVectorLength = math.length(characterControl.MoveVector);
        
        return state switch
        {
            CharacterState.GroundMove => GetGroundMoveAnimation(
                characterComponent, moveVectorLength, velocityMagnitude),
                
            CharacterState.Crouched => GetCrouchedAnimation(
                characterComponent, moveVectorLength, velocityMagnitude),
                
            CharacterState.Swimming => GetSwimmingAnimation(
                characterComponent, velocityMagnitude),
                
            CharacterState.Climbing => new AnimationData(
                PlatformerCharacterAnimation.ClimbingMoveClip, 
                velocityMagnitude / characterComponent.ClimbingSpeed),
                
            CharacterState.LedgeGrab => new AnimationData(
                PlatformerCharacterAnimation.LedgeGrabMoveClip,
                velocityMagnitude / characterComponent.LedgeMoveSpeed),
                
            CharacterState.WallRun => GetWallRunAnimation(localTransform, characterComponent),
            
            CharacterState.AirMove => new AnimationData(PlatformerCharacterAnimation.InAirClip),
            CharacterState.Dashing => new AnimationData(PlatformerCharacterAnimation.DashClip),
            CharacterState.RopeSwing => new AnimationData(PlatformerCharacterAnimation.RopeHangClip),
            CharacterState.LedgeStandingUp => new AnimationData(PlatformerCharacterAnimation.LedgeStandUpClip),
            CharacterState.Sliding => new AnimationData(PlatformerCharacterAnimation.SlidingClip),
            CharacterState.Rolling or CharacterState.FlyingNoCollisions => 
                new AnimationData(PlatformerCharacterAnimation.IdleClip),
            
            _ => new AnimationData(PlatformerCharacterAnimation.IdleClip)
        };
    }

    [BurstCompile]
    private static AnimationData GetGroundMoveAnimation(
        in PlatformerCharacterComponent characterComponent,
        float moveVectorLength,
        float velocityMagnitude)
    {
        if (moveVectorLength < 0.01f)
        {
            return new AnimationData(PlatformerCharacterAnimation.IdleClip);
        }

        if (characterComponent.IsSprinting)
        {
            var velocityRatio = velocityMagnitude / characterComponent.GroundSprintMaxSpeed;
            return new AnimationData(PlatformerCharacterAnimation.SprintClip, velocityRatio);
        }
        else
        {
            var velocityRatio = velocityMagnitude / characterComponent.GroundRunMaxSpeed;
            return new AnimationData(PlatformerCharacterAnimation.RunClip, velocityRatio);
        }
    }

    [BurstCompile]
    private static AnimationData GetCrouchedAnimation(
        in PlatformerCharacterComponent characterComponent,
        float moveVectorLength,
        float velocityMagnitude)
    {
        if (moveVectorLength < 0.01f)
        {
            return new AnimationData(PlatformerCharacterAnimation.CrouchIdleClip);
        }

        var velocityRatio = velocityMagnitude / characterComponent.CrouchedMaxSpeed;
        return new AnimationData(PlatformerCharacterAnimation.CrouchMoveClip, velocityRatio);
    }

    [BurstCompile]
    private static AnimationData GetSwimmingAnimation(
        in PlatformerCharacterComponent characterComponent,
        float velocityMagnitude)
    {
        var velocityRatio = velocityMagnitude / characterComponent.SwimmingMaxSpeed;
        
        if (velocityRatio < 0.1f)
        {
            return new AnimationData(PlatformerCharacterAnimation.SwimmingIdleClip);
        }
        
        return new AnimationData(PlatformerCharacterAnimation.SwimmingMoveClip, velocityRatio);
    }

    [BurstCompile]
    private static AnimationData GetWallRunAnimation(
        in LocalTransform localTransform,
        in PlatformerCharacterComponent characterComponent)
    {
        var wallIsOnTheLeft = math.dot(
            MathUtilities.GetRightFromRotation(localTransform.Rotation),
            characterComponent.LastKnownWallNormal) > 0f;
            
        var clipId = wallIsOnTheLeft 
            ? PlatformerCharacterAnimation.WallRunLeftClip 
            : PlatformerCharacterAnimation.WallRunRightClip;
            
        return new AnimationData(clipId);
    }

    private static void SetAnimation(
        Animator animator,
        ref PlatformerCharacterAnimation characterAnimation,
        in PlatformerCharacterStateMachine characterStateMachine,
        in CarryingComponent carryingComponent,
        in AnimationData animationData)
    {
        // Calculate carrying modifiers
        var loadRatio = carryingComponent.GetLoadRatio();
        var animationSpeedMultiplier = carryingComponent.ComputeAnimationSpeedMultiplier();
        var finalSpeed = animationData.baseSpeed * animationSpeedMultiplier;
        
        // Set basic animation parameters
        animator.SetInteger(ClipIndex, animationData.clipIndex);
        animator.speed = finalSpeed;
        
        // Set carrying-related parameters for animator
        animator.SetFloat(LoadRatio, loadRatio);
        animator.SetFloat(CarryingWeight, carryingComponent.currentWeight);
        
        // Set breathing state based on load
        var isBreathingHeavy = loadRatio >= characterAnimation.heavyBreathingLoadRatio;
        animator.SetBool(IsBreathingHeavy, isBreathingHeavy);
        
        // Update tracking variables
        characterAnimation.LastAnimationCharacterState = characterStateMachine.CurrentState;
        characterAnimation.LastLoadRatio = loadRatio;
        
        // Optional: Add debug info in development builds
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (math.abs(loadRatio - characterAnimation.LastLoadRatio) > 0.1f)
        {
            Debug.Log($"Load ratio changed significantly: {characterAnimation.LastLoadRatio:F2} -> {loadRatio:F2}, Speed modifier: {animationSpeedMultiplier:F2}");
        }
        #endif
    }

    // Utility methods for external systems
    [BurstCompile]
    public static bool ShouldPlayBreathingSound(in CarryingComponent carryingComponent, in PlatformerCharacterAnimation characterAnimation)
    {
        return carryingComponent.GetLoadRatio() >= characterAnimation.breathingIntensityThreshold;
    }

    [BurstCompile]
    public static float GetBreathingIntensity(in CarryingComponent carryingComponent)
    {
        return carryingComponent.ComputeBreathingIntensity();
    }

    [BurstCompile]
    public static float GetFootstepVolumeMultiplier(in CarryingComponent carryingComponent)
    {
        return carryingComponent.ComputeFootstepVolume();
    }
}