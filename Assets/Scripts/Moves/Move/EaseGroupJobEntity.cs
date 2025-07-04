using Eases.Ease.Data;
using Moves.Move.Data;
using Moves.Move.Data.Blobs;
using Times.Time.Data;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Moves.Move
{
    [BurstCompile]
    public unsafe partial struct EaseGroupJobChunk : IJobChunk
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public NativeArray<float3>.ReadOnly EaseCache;
        
        public ComponentTypeHandle<LocalTransform> LocalTransformHandle;
        public ComponentTypeHandle<EaseStepPlanComponent> EaseStepPlanHandle;
        
        [ReadOnly] public ComponentTypeHandle<EaseComponent> EaseHandle;
        [ReadOnly] public ComponentTypeHandle<AliveTimeComponent> AliveTimeHandle;
        [WriteOnly] public float TimePassed;

        [BurstCompile]
        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            var localTransforms = chunk.GetNativeArray(ref LocalTransformHandle);
            var easeComponents = chunk.GetNativeArray(ref EaseHandle);
            var aliveTimeComponents = chunk.GetNativeArray(ref AliveTimeHandle);
            var easeStepPlan = chunk.GetNativeArray(ref EaseStepPlanHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            while (enumerator.NextEntityIndex(out int i))
            {
                var ease = easeComponents[i];
                var aliveTime = aliveTimeComponents[i];
                var current = easeStepPlan[i].Current;

                if (!ease.TryEvaluate(aliveTime.Value, 5, DeltaTime, out var t))
                {
                    return;
                }
                Debug.Log($"({TimePassed}, {t})");

                var local = localTransforms[i];
                switch (ease.Leading3Bit)
                {
                    // case 0:
                    //     local.Position.x = math.lerp(moveStartComponents[i].Value.x, moveEndComponents[i].Value.x, t);
                    //     break;
                    //
                    // case 1:
                    //     local.Position.y = math.lerp(moveStartComponents[i].Value.y, moveEndComponents[i].Value.y, t);
                    //     break;
                    //
                    // case 2:
                    //     local.Position.z = math.lerp(moveStartComponents[i].Value.z, moveEndComponents[i].Value.z, t);
                    //     break;
                    //
                    case 3:

                        
                        var start = EaseCache[current];
                        var end = EaseCache[current + 1];
                        local.Position = math.lerp(start, end, t);
                        break;

                    // case 4:
                    //     var angleX = math.lerp(startRots[i].Value, endRots[i].Value, t);
                    //     local.Rotation = quaternion.RotateX(angleX);
                    //     break;
                    //
                    // case 5:
                    //     var angleY = math.lerp(startRots[i].Value, endRots[i].Value, t);
                    //     local.Rotation = quaternion.RotateY(angleY);
                    //     break;
                    //
                    // case 6:
                    //     var angleZ = math.lerp(startRots[i].Value, endRots[i].Value, t);
                    //     local.Rotation = quaternion.RotateZ(angleZ);
                    //     break;
                    //
                    // case 7:
                    //     break;
                }

                localTransforms[i] = local;
            }
        }
    }
}