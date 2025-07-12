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


        [BurstCompile]
        public readonly void GetAnimationState(
            float2 moveVector,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic,
            bool isSprinting,
            float crouchedMaxSpeed,
            float climbingSpeed,
            float ledgeMoveSpeed,
            float swimmingMaxSpeed,
            quaternion rotation,
            float3 lastKnownWallNormal,
            out AnimationStateComponent animationStateComponent
        )
        {
            switch (Current)
            {
                case ECharacterState.GroundMove:
                {
                    if (math.lengthsq(moveVector) < 0.0001f)
                    {
                        animationStateComponent.Speed = new half(1);
                        animationStateComponent.Animation = EAnimationState.Idle;
                    }
                    else
                    {
                        if (isSprinting)
                        {
                            var velocityRatio =
                                (half)(VelocityMagnitude(ref intrinsic) / GroundSprintMaxSpeed(ref stats));
                            animationStateComponent.Speed = velocityRatio;
                            animationStateComponent.Animation = EAnimationState.Sprint;
                        }
                        else
                        {
                            var velocityRatio =
                                (half)(VelocityMagnitude(ref intrinsic) / GroundSprintMaxSpeed(ref stats));
                            animationStateComponent.Speed = velocityRatio;
                            animationStateComponent.Animation = EAnimationState.Run;
                        }
                    }
                }
                    break;
                case ECharacterState.Crouched:
                {
                    if (math.lengthsq(moveVector) < 0.0001f)
                    {
                        animationStateComponent.Speed = new half(1);
                        animationStateComponent.Animation = EAnimationState.CrouchIdle;
                    }
                    else
                    {
                        half velocityRatio = (half)(VelocityMagnitude(ref intrinsic) / crouchedMaxSpeed);
                        animationStateComponent.Speed = velocityRatio;
                        animationStateComponent.Animation = EAnimationState.CrouchMove;
                    }
                }
                    break;
                case ECharacterState.AirMove:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.InAir;
                }
                    break;
                case ECharacterState.Dashing:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.Dash;
                }
                    break;
                case ECharacterState.WallRun:
                {
                    float3 rightVector = math.mul(rotation, new float3(1f, 0f, 0f));
                    bool wallIsOnTheLeft = math.dot(rightVector, lastKnownWallNormal) > 0f;
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation =
                        wallIsOnTheLeft ? EAnimationState.WallRunLeft : EAnimationState.WallRunRight;
                }
                    break;
                case ECharacterState.RopeSwing:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.RopeHang;
                }
                    break;
                case ECharacterState.Climbing:
                {
                    half velocityRatio =
                        (half)(climbingSpeed > 0f ? VelocityMagnitude(ref intrinsic) / climbingSpeed : 0f);
                    animationStateComponent.Speed = velocityRatio;
                    animationStateComponent.Animation = EAnimationState.ClimbingMove;
                }
                    break;
                case ECharacterState.LedgeGrab:
                {
                    half velocityRatio =
                        (half)(ledgeMoveSpeed > 0f ? VelocityMagnitude(ref intrinsic) / ledgeMoveSpeed : 0f);
                    animationStateComponent.Speed = velocityRatio;
                    animationStateComponent.Animation = EAnimationState.LedgeGrabMove;
                }
                    break;
                case ECharacterState.LedgeStandingUp:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.LedgeStandUp;
                }
                    break;
                case ECharacterState.Swimming:
                {
                    float velocityRatio =
                        swimmingMaxSpeed > 0f ? VelocityMagnitude(ref intrinsic) / swimmingMaxSpeed : 0f;
                    if (velocityRatio < 0.1f)
                    {
                        animationStateComponent.Speed = new half(1);
                        animationStateComponent.Animation = EAnimationState.SwimmingIdle;
                    }
                    else
                    {
                        animationStateComponent.Speed = (half)velocityRatio;
                        animationStateComponent.Animation = EAnimationState.SwimmingMove;
                    }
                }
                    break;
                case ECharacterState.Sliding:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.Sliding;
                }
                    break;
                case ECharacterState.Rolling:
                case ECharacterState.FlyingNoCollisions:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.Idle;
                }
                    break;
                default:
                {
                    animationStateComponent.Speed = new half(1);
                    animationStateComponent.Animation = EAnimationState.Idle;
                }
                    break;
            }
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