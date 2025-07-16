using System;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Mathematics;
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
            in float2 moveDirection,
            float deltaTime,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        )
        {

            var velocityMagnitude = intrinsic.GetValue(EIntrinsic.Speed.ToKey(out var factor)) / factor;
            var normalize = math.normalize(moveDirection);
            // localTransform.Position.xz += normalize * velocityMagnitude * deltaTime;

            // Only update rotation if there is movement to avoid snapping back to a default rotation.
            if (math.abs(moveDirection.x)> 0.01f)
            {
                var moveDirectionF3 = new float3(moveDirection.x, 0f, moveDirection.y);
                // localTransform.Rotation = quaternion.LookRotation(moveDirectionF3, math.up());
            }
        }
    }
}