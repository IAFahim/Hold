using System;
using Animations.Animation.Data;
using BovineLabs.Core.Input;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using Unity.Burst;
using Unity.Transforms;

namespace States.States.Data
{
    [Serializable]
    [BurstCompile]
    public struct GroundMoveState
    {

        [BurstCompile]
        public static void OnStateEnter(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            in InputComponent inputComponent,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        )
        {
            var moveDelta = inputComponent.MoveDelta;
            localTransform.Position.xz += moveDelta;
        }
    }
}