using System;
using System.Runtime.CompilerServices;
using Animations.Animation.Data;
using BovineLabs.Core.Input;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using Inputs.Inputs.Data;
using Moves.Move.Data;
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
            new StateJobEntity
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
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
        [ReadOnly] public float DeltaTime;

        [BurstCompile]
        private void Execute(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            ref CharacterInputComponent characterInput,
            ref AnimationStateComponent animationState,
            ref DynamicBuffer<Stat> stats,
            ref DynamicBuffer<Intrinsic> intrinsic
        )
        {
            var statsMap = stats.AsMap();
            var intrinsicMap = intrinsic.AsMap();
            Apply3LineConstrain(ref characterInput, ref localTransform, out var moveDirection);
            OnStart(ref localTransform, ref characterState, moveDirection, ref statsMap, ref intrinsicMap);
            characterState.GetAnimationState(moveDirection, ref statsMap, ref intrinsicMap,
                characterInput.IsSprinting(),
                localTransform.Rotation, new float3(0, 0, 0), out var animationStateComponent);
            animationState = animationStateComponent;
        }


        private static readonly float LaneBoundary = 2.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Apply3LineConstrain(
            ref CharacterInputComponent characterInput,
            ref LocalTransform localTransform,
            out float2 moveDirection
        )
        {
            moveDirection = new(0, 1);
            var positionX = localTransform.Position.x;
            var target = 0f;
            if (characterInput.IsGoingToLeftLine())
                target = -LaneBoundary;
            else if (characterInput.IsGoingToMiddleLine())
            {
                target = 0;
            }
            else if (characterInput.IsGoingToRightLine())
            {
                target = LaneBoundary;
            }


            moveDirection.x = -positionX + target;
            if (positionX < -LaneBoundary)
            {
                localTransform.Position.x = -LaneBoundary;
                return;
            }

            if (positionX > LaneBoundary)
            {
                localTransform.Position.x = LaneBoundary;
                return;
            }
        }

        private void OnStart(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            in float2 moveDirection,
            ref DynamicHashMap<StatKey, StatValue> statsMap,
            ref DynamicHashMap<IntrinsicKey, int> intrinsicMap
        )
        {
            switch (characterState.Current)
            {
                case ECharacterState.Uninitialized:
                    break;
                case ECharacterState.GroundMove:
                    GroundMoveState.OnStateEnter(
                        ref localTransform,
                        moveDirection,
                        DeltaTime,
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