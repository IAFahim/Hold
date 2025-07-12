using System.Runtime.CompilerServices;
using Animations.Animation.Data;
using Animations.Animation.Data.enums;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using States.States.Data.enums;
using StatsHelpers.StatsHelpers.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace States.States.Data
{
    [BurstCompile]
    public struct CharacterStateComponent : IComponentData
    {
        public ECharacterState Previous;
        public ECharacterState Current;

        /// <summary>
        /// Determines the appropriate animation state based on the character's current state and dynamics.
        /// </summary>
        [BurstCompile]
        public readonly void GetAnimationState(
            float2 moveVector,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic,
            bool isSprinting,
            quaternion rotation,
            float3 lastKnownWallNormal,
            out AnimationStateComponent animationState)
        {
            switch (Current)
            {
                case ECharacterState.GroundMove:
                    animationState = GetGroundMoveAnimationState(moveVector, isSprinting, ref intrinsic, ref stats);
                    break;
                case ECharacterState.Crouched:
                    animationState = GetCrouchedAnimationState(moveVector, ref intrinsic, ref stats);
                    break;
                case ECharacterState.WallRun:
                    animationState = GetWallRunAnimationState(rotation, lastKnownWallNormal);
                    break;
                case ECharacterState.Climbing:
                    animationState = GetClimbingAnimationState(ref intrinsic, ref stats);
                    break;
                case ECharacterState.LedgeGrab:
                    animationState = GetLedgeGrabAnimationState(ref intrinsic, ref stats);
                    break;
                case ECharacterState.Swimming:
                    animationState = GetSwimmingAnimationState(ref intrinsic, ref stats);
                    break;
                case ECharacterState.AirMove:
                    animationState = GetStaticAnimationState(EAnimationState.InAir);
                    break;
                case ECharacterState.Dashing:
                    animationState = GetStaticAnimationState(EAnimationState.Dash);
                    break;
                case ECharacterState.RopeSwing:
                    animationState = GetStaticAnimationState(EAnimationState.RopeHang);
                    break;
                case ECharacterState.LedgeStandingUp:
                    animationState = GetStaticAnimationState(EAnimationState.LedgeStandUp);
                    break;
                case ECharacterState.Sliding:
                    animationState = GetStaticAnimationState(EAnimationState.Sliding);
                    break;
                case ECharacterState.Rolling:
                case ECharacterState.FlyingNoCollisions:
                default:
                    animationState = GetStaticAnimationState(EAnimationState.Idle);
                    break;
            }
        }

        [BurstCompile]
        private readonly AnimationStateComponent GetGroundMoveAnimationState(float2 moveVector, bool isSprinting,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic, ref DynamicHashMap<StatKey, StatValue> stats)
        {
            if (math.lengthsq(moveVector) < 0.0001f)
            {
                return GetStaticAnimationState(EAnimationState.Idle);
            }
        
            var velocityRatio = (half)(VelocityMagnitude(ref intrinsic) / GroundSprintMaxSpeed(ref stats));
            return new AnimationStateComponent
            {
                Speed = velocityRatio,
                Animation = isSprinting ? EAnimationState.Sprint : EAnimationState.Run,
            };
        }
        
        [BurstCompile]
        private readonly AnimationStateComponent GetCrouchedAnimationState(float2 moveVector,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic, ref DynamicHashMap<StatKey, StatValue> stats)
        {
            if (math.lengthsq(moveVector) < 0.0001f)
            {
                return GetStaticAnimationState(EAnimationState.CrouchIdle);
            }
        
            var crouchedMaxSpeed = stats.GetValue(EStat.CrouchedMaxSpeed.ToKey());
            var velocityRatio = (half)(crouchedMaxSpeed > 0 ? VelocityMagnitude(ref intrinsic) / crouchedMaxSpeed : 0);
            return new AnimationStateComponent
            {
                Speed = velocityRatio,
                Animation = EAnimationState.CrouchMove,
            };
        }
        
        [BurstCompile]
        private readonly AnimationStateComponent GetWallRunAnimationState(quaternion rotation,
            float3 lastKnownWallNormal)
        {
            var rightVector = math.mul(rotation, new float3(1f, 0f, 0f));
            var wallIsOnTheLeft = math.dot(rightVector, lastKnownWallNormal) > 0f;
            return GetStaticAnimationState(wallIsOnTheLeft
                ? EAnimationState.WallRunLeft
                : EAnimationState.WallRunRight);
        }
        
        [BurstCompile]
        private readonly AnimationStateComponent GetClimbingAnimationState(
            ref DynamicHashMap<IntrinsicKey, int> intrinsic,
            ref DynamicHashMap<StatKey, StatValue> stats)
        {
            var climbingSpeed = stats.GetValue(EStat.ClimbingSpeed.ToKey());
            var velocityRatio = (half)(climbingSpeed > 0f ? VelocityMagnitude(ref intrinsic) / climbingSpeed : 0f);
            return new AnimationStateComponent
            {
                Speed = velocityRatio,
                Animation = EAnimationState.ClimbingMove,
            };
        }
        
        [BurstCompile]
        private readonly AnimationStateComponent GetLedgeGrabAnimationState(
            ref DynamicHashMap<IntrinsicKey, int> intrinsic,
            ref DynamicHashMap<StatKey, StatValue> stats)
        {
            var ledgeMoveSpeed = stats.GetValue(EStat.LedgeMoveSpeed.ToKey());
            var velocityRatio = (half)(ledgeMoveSpeed > 0f ? VelocityMagnitude(ref intrinsic) / ledgeMoveSpeed : 0f);
            return new AnimationStateComponent
            {
                Speed = velocityRatio,
                Animation = EAnimationState.LedgeGrabMove,
            };
        }
        
        [BurstCompile]
        private readonly AnimationStateComponent GetSwimmingAnimationState(
            ref DynamicHashMap<IntrinsicKey, int> intrinsic,
            ref DynamicHashMap<StatKey, StatValue> stats)
        {
            var swimmingMaxSpeed = stats.GetValue(EStat.SwimmingMaxSpeed.ToKey());
            var velocityRatio = swimmingMaxSpeed > 0f ? VelocityMagnitude(ref intrinsic) / swimmingMaxSpeed : 0f;
        
            if (velocityRatio < 0.1f)
            {
                return GetStaticAnimationState(EAnimationState.SwimmingIdle);
            }
        
            return new AnimationStateComponent
            {
                Speed = (half)velocityRatio,
                Animation = EAnimationState.SwimmingMove,
            };
        }
        
        [BurstCompile]
        private readonly AnimationStateComponent GetStaticAnimationState(EAnimationState animation)
        {
            return new AnimationStateComponent
            {
                Speed = new half(1),
                Animation = animation,
            };
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly float VelocityMagnitude(ref DynamicHashMap<IntrinsicKey, int> intrinsic)
        {
            return intrinsic.GetValue(EIntrinsic.Speed.ToKey(out var factor)) / factor;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly float GroundSprintMaxSpeed(ref DynamicHashMap<StatKey, StatValue> stat)
        {
            return stat.GetValue(EStat.GroundSprintMaxSpeed.ToKey());
        }
    }
}