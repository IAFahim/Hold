using System;
using Animations.Animation.Data;
using BovineLabs.Core.Input;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using States.States.Data;
using States.States.Data.enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            var inputComponent = SystemAPI.GetSingleton<InputComponent>();
            new StateJobEntity
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                InputComponent = inputComponent
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct StateJobEntity : IJobEntity
    {
        [ReadOnly] public InputComponent InputComponent;
        [ReadOnly]  public float DeltaTime;

        [BurstCompile]
        private void Execute(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            ref AnimationStateComponent animationState,
            ref DynamicBuffer<Stat> stats,
            ref DynamicBuffer<Intrinsic> intrinsic
        )
        {
            var statsMap = stats.AsMap();
            var intrinsicMap = intrinsic.AsMap();
            OnStart(ref localTransform, ref characterState, ref statsMap, ref intrinsicMap);
            characterState.GetAnimationState(InputComponent.Move, ref statsMap, ref intrinsicMap, false, 10, 10, 10, 10, 10, localTransform.Rotation, new float3(0, 0, 0), out var animationStateComponent);
            animationState = animationStateComponent;
        }

        private void OnStart(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            ref DynamicHashMap<StatKey, StatValue> statsMap,
            ref DynamicHashMap<IntrinsicKey, int> intrinsicMap
        )
        {
            switch (characterState.Current)
            {
                case ECharacterState.Uninitialized:
                    break;
                case ECharacterState.GroundMove:
                    GroundMoveState.OnStateEnter(ref localTransform,
                        ref characterState,
                        InputComponent.Move, DeltaTime,
                        ref statsMap, ref intrinsicMap
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