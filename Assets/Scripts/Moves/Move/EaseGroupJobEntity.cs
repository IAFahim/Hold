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
        [ReadOnly] public NativeArray<float>.ReadOnly PlaneRotationCache;
        [ReadOnly] public NativeArray<float>.ReadOnly UniformScaleCache;


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
                    EaseLerpPositionX(ref transform, easeState, currentConfig, t);
                    break;
                
                case 1:
                    EaseLerpPositionY(ref transform, easeState, currentConfig, t);
                    break;

                case 2:
                    EaseLerpPositionZ(ref transform, easeState, currentConfig, t);
                    break;

                case 3:
                    EaseLerpPositionXYZ(ref transform, easeState, currentConfig, t);
                    break;

                case 4:
                    EaseLerpRotationX(ref transform, easeState, currentConfig, t);
                    break;
                
                case 5:
                    EaseLerpRotationY(ref transform, easeState, currentConfig, t);
                    break;
                
                case 6:
                    EaseLerpRotationZ(ref transform, easeState, currentConfig, t);
                    break;
                
                case 7:
                    EaseLerpUniformScale(ref transform, easeState, currentConfig, t);
                    break;
            }

            if (isComplete)
            {
                easeState.Current = currentConfig.Next;
                easeState.ElapsedTime = 0f; // Reset timer for the next ease
            }
        }
        
        // --- Mapping Methods ---

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseAxesMap(EaseStateComponent easeState, EaseCache currentConfig, out float start, out float end)
        {
            start = AxesCache[easeState.Current];
            end = AxesCache[currentConfig.Next];
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
        private void EasePlaneRotationMap(EaseStateComponent easeState, EaseCache currentConfig, out float start, out float end)
        {
            start = PlaneRotationCache[easeState.Current];
            end = PlaneRotationCache[currentConfig.Next];
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseUniformScaleMap(EaseStateComponent easeState, EaseCache currentConfig, out float start, out float end)
        {
            start = UniformScaleCache[easeState.Current];
            end = UniformScaleCache[currentConfig.Next];
        }

        // --- Position Easing ---

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpPositionX(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EaseAxesMap(easeState, currentConfig, out var start, out var end);
            transform.Position.x = math.lerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpPositionY(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EaseAxesMap(easeState, currentConfig, out var start, out var end);
            transform.Position.y = math.lerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpPositionZ(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EaseAxesMap(easeState, currentConfig, out var start, out var end);
            transform.Position.z = math.lerp(start, end, t);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpPositionXYZ(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EaseMapFloat3(easeState, currentConfig, out var start, out var end);
            transform.Position = math.lerp(start, end, t);
        }
        
        // --- Rotation Easing ---

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpRotationX(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EasePlaneRotationMap(easeState, currentConfig, out var start, out var end);
            transform.Rotation = quaternion.RotateX(math.lerp(start, end, t));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpRotationY(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EasePlaneRotationMap(easeState, currentConfig, out var start, out var end);
            transform.Rotation = quaternion.RotateY(math.lerp(start, end, t));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpRotationZ(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EasePlaneRotationMap(easeState, currentConfig, out var start, out var end);
            transform.Rotation = quaternion.RotateZ(math.lerp(start, end, t));
        }
        
        // --- Scale Easing ---

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EaseLerpUniformScale(ref LocalTransform transform, EaseStateComponent easeState, EaseCache currentConfig, float t)
        {
            EaseUniformScaleMap(easeState, currentConfig, out var start, out var end);
            transform.Scale = math.lerp(start, end, t);
        }
    }
}