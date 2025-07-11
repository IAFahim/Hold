using System;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace States.States.Data
{
    [Serializable]
    [BurstCompile]
    public struct GroundMoveState
    {
        public float velocity;

        [BurstCompile]
        public static void OnStateEnter(ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            in float2 moveDelta,
            float deltaTime,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        )
        {
            var velocityMagnitude = intrinsic.GetValue(1) / 1000;
            var normalize = math.normalize(moveDelta);
            localTransform.Position.xz += normalize * velocityMagnitude * deltaTime;

            // Only update rotation if there is movement to avoid snapping back to a default rotation.
            if (math.lengthsq(moveDelta) > 0.0001f)
            {
                var moveDirection = new float3(moveDelta.x, 0f, moveDelta.y);
                localTransform.Rotation = quaternion.LookRotation(moveDirection, math.up());
            }
        }
    }
}