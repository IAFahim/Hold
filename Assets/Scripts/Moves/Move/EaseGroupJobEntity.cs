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
        [ReadOnly] public NativeArray<EaseCache>.ReadOnly EaseCache;
        [ReadOnly] public NativeArray<float3>.ReadOnly PositionCache;
        [ReadOnly] public NativeArray<float>.ReadOnly AxesCache;


        [BurstCompile]
        public void Execute(
            ref LocalTransform transform,
            ref EaseStateComponent easeState
        )
        {
            var currentConfig = EaseCache[easeState.Current];
            var isComplete = currentConfig.Ease.TryComplete(
                ref easeState.ElapsedTime,
                currentConfig.Duration,
                DeltaTime,
                out var t
            );

            switch (currentConfig.Ease.Leading3Bit())
            {
                case 0:
                    EaseLerpX(ref transform, easeState, currentConfig, t);
                    break;
                
                case 1:
                    EaseLerpY(ref transform, easeState, currentConfig, t);
                    break;

                case 2:
                    EaseLerpZ(ref transform, easeState, currentConfig, t);
                    break;

                case 3:
                    EaseLerpPosition(ref transform, easeState, currentConfig, t);
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

            if (isComplete) easeState.Current = currentConfig.Next;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseMap(EaseStateComponent easeState, EaseCache currentConfig, out float start, out float end)
        {
            start = AxesCache[easeState.Current];
            end = AxesCache[currentConfig.Next];
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpX(ref LocalTransform transform,
            EaseStateComponent easeState,
            EaseCache currentConfig,
            float t
        )
        {
            EaseMap(easeState, currentConfig, out var start, out var end);
            transform.Position.x = math.lerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpY(ref LocalTransform transform,
            EaseStateComponent easeState,
            EaseCache currentConfig,
            float t
        )
        {
            EaseMap(easeState, currentConfig, out var start, out var end);
            transform.Position.y = math.lerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpZ(ref LocalTransform transform,
            EaseStateComponent easeState,
            EaseCache currentConfig,
            float t
        )
        {
            EaseMap(easeState, currentConfig, out var start, out var end);
            transform.Position.z = math.lerp(start, end, t);
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseMapFloat3(EaseStateComponent easeState, EaseCache currentConfig, out float3 start, out float3 end)
        {
            start = PositionCache[easeState.Current];
            end = PositionCache[currentConfig.Next];
        }


        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpPosition(
            ref LocalTransform transform,
            EaseStateComponent easeState,
            EaseCache currentConfig,
            float t
        )
        {
            EaseMapFloat3(easeState, currentConfig, out var start, out float3 end);
            transform.Position = math.lerp(start, end, t);
        }

        
    }
}