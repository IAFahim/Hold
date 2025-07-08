using System.Runtime.CompilerServices;
using Moves.Move.Data.Blobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Moves.Move
{
    [BurstCompile]
    public partial struct EaseGroupJobChunk : IJobEntity
    {
        [ReadOnly] public float DeltaTime;

        [BurstCompile]
        private void Execute(
            in BlobEaseCacheComponent ease,
            ref LocalTransform transform,
            ref EaseStateComponent easeState
        )
        {
            var current = easeState.Current;
            
            ref var easeCacheBlob = ref ease.Blob.Value;
            var currentConfig = easeCacheBlob.Cache[current];
            var isComplete = currentConfig.Ease.TryComplete(
                ref easeState.ElapsedTime,
                currentConfig.Duration,
                DeltaTime,
                out var t
            );

            var leading3Bit = currentConfig.Ease.Leading3Bit();

            if ((leading3Bit & 0b001) != 0)
            {
                EaseLerpPositionXYZ(ref transform, ref easeCacheBlob, current, t);
            }

            if ((leading3Bit & 0b010) != 0)
            {
                EaseLerpRotation(ref transform, ref easeCacheBlob, current, t);
            }

            if ((leading3Bit & 0b100) != 0)
            {
                EaseLerpUniformScale(ref transform, ref easeCacheBlob, current, t);
            }

            if (!isComplete) return;
            easeState.Current = currentConfig.Next;
            easeState.ElapsedTime = 0f; // Reset timer for the next ease
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpPositionXYZ(
            ref LocalTransform transform,
            ref EaseCacheBlob easeCacheBlob,
            in byte current,
            float t
        )
        {
            var next = easeCacheBlob.Cache[current].Next;
            var start = easeCacheBlob.Positions[current];
            var end = easeCacheBlob.Positions[next];
            transform.Position = math.lerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpRotation(
            ref LocalTransform transform,
            ref EaseCacheBlob easeCacheBlob,
            in byte current,
            float t
        )
        {
            var next = easeCacheBlob.Cache[current].Next;
            var start = easeCacheBlob.Quaternion[current];
            var end = easeCacheBlob.Quaternion[next];
            transform.Rotation = math.slerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpUniformScale(
            ref LocalTransform transform,
            ref EaseCacheBlob easeCacheBlob,
            in byte current,
            float t
        )
        {
            var next = easeCacheBlob.Cache[current].Next;
            var start = easeCacheBlob.Scale[current];
            var end = easeCacheBlob.Scale[next];
            transform.Scale = math.lerp(start, end, t);
        }
    }
}