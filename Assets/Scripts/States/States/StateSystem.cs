using System;
using Animations.Animation.Data;
using BovineLabs.Core.Input;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using States.States.Data;
using States.States.Data.enums;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace States.States
{
    public partial struct StateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new StateJobEntity().ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct StateJobEntity : IJobEntity
    {
        [BurstCompile]
        private void Execute(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            in AnimationStateComponent animationState,
            in InputComponent inputComponent,
            ref DynamicBuffer<Stat> stats,
            ref DynamicBuffer<Intrinsic> intrinsic
        )
        {
            var statsMap = stats.AsMap();
            var intrinsicMap = intrinsic.AsMap();
            switch (characterState.Current)
            {
                case ECharacterState.Uninitialized:
                    break;
                case ECharacterState.GroundMove:
                    GroundMoveState.OnStateEnter(
                        ref localTransform,
                        ref characterState,
                        inputComponent,
                        ref statsMap,
                        ref intrinsicMap
                    );
                    break;
                case ECharacterState.Crouched:
                    break;
                case ECharacterState.AirMove:
                    break;
                case ECharacterState.WallRun:
                    break;
                case ECharacterState.Rolling:
                    break;
                case ECharacterState.LedgeGrab:
                    break;
                case ECharacterState.LedgeStandingUp:
                    break;
                case ECharacterState.Dashing:
                    break;
                case ECharacterState.Swimming:
                    break;
                case ECharacterState.Climbing:
                    break;
                case ECharacterState.FlyingNoCollisions:
                    break;
                case ECharacterState.RopeSwing:
                    break;
                case ECharacterState.Sliding:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}