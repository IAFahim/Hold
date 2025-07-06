using BovineLabs.Core.SingletonCollection;
using Eases.Ease.Data;
using Moves.Move.Data;
using Moves.Move.Data.Blobs;
using Times.Time.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Moves.Move
{
    [BurstCompile]
    public partial struct MoveSystem : ISystem, ISystemStartStop
    {
        private NativeArray<EaseCache> _easeCaches;
        private NativeArray<float3> _positionCaches;
        private NativeArray<float> _axesCaches;
        private NativeArray<float> _planeRotationCaches;
        private NativeArray<float> _uniformScaleRotationCaches;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            _easeCaches.Dispose();
            _positionCaches.Dispose();
            _axesCaches.Dispose();
            _planeRotationCaches.Dispose();
            _uniformScaleRotationCaches.Dispose();
        }


        [BurstCompile]
        public unsafe void OnUpdate(ref SystemState state)
        {
            if (!_easeCaches.IsCreated)
            {
                ref var easeCacheBlob = ref SystemAPI.GetSingleton<BlobEaseCacheComponent>().Blob.Value.Cache;
                var easeCacheLength = easeCacheBlob.Length;

                _easeCaches = new NativeArray<EaseCache>(easeCacheLength, Allocator.Persistent);
                for (int i = 0; i < easeCacheLength; i++) _easeCaches[i] = easeCacheBlob[i];
            }

            if (!_positionCaches.IsCreated)
            {
                ref var positionCacheBlob =
                    ref SystemAPI.GetSingleton<BlobPositionCacheComponent>().Blob.Value.Positions;
                var positionCacheLenght = positionCacheBlob.Length;

                _positionCaches = new NativeArray<float3>(positionCacheLenght, Allocator.Persistent);
                for (int i = 0; i < positionCacheLenght; i++) _positionCaches[i] = positionCacheBlob[i];
            }

            if (!_planeRotationCaches.IsCreated)
            {
                ref var planeRotationCacheBlob =
                    ref SystemAPI.GetSingleton<BlobPlaneRotationCacheComponent>().Blob.Value.Radians;
                var planeRotationCacheLength = planeRotationCacheBlob.Length;

                _planeRotationCaches = new NativeArray<float>(planeRotationCacheLength, Allocator.Persistent);
                for (int i = 0; i < planeRotationCacheLength; i++) _planeRotationCaches[i] = planeRotationCacheBlob[i];
            }

            if (!_axesCaches.IsCreated)
            {
                ref var axesCacheBlob = ref SystemAPI.GetSingleton<BlobAxesCacheComponent>().Blob.Value.Axes;
                var axesCacheLength = axesCacheBlob.Length;

                _axesCaches = new NativeArray<float>(axesCacheLength, Allocator.Persistent);
                for (int i = 0; i < axesCacheLength; i++) _axesCaches[i] = axesCacheBlob[i];
            }

            if (!_uniformScaleRotationCaches.IsCreated)
            {
                ref var uniformCacheBlob = ref SystemAPI.GetSingleton<BlobUniformScaleComponent>().Blob.Value.Scale;
                var uniformCacheBlobLength = uniformCacheBlob.Length;

                _uniformScaleRotationCaches = new NativeArray<float>(uniformCacheBlobLength, Allocator.Persistent);
                for (int i = 0; i < uniformCacheBlobLength; i++) _uniformScaleRotationCaches[i] = uniformCacheBlob[i];
            }


            new EaseGroupJobChunk
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                EaseCache = _easeCaches.AsReadOnly(),
                PositionCache = _positionCaches.AsReadOnly(),
                AxesCache = _axesCaches.AsReadOnly(),
                PlaneRotationCache = _planeRotationCaches.AsReadOnly(),
                UniformScaleCache = _uniformScaleRotationCaches.AsReadOnly()
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
        }
    }
}