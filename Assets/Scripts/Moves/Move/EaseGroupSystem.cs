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
    public partial struct MoveSystem : ISystem
    {
        private EntityQuery _query;
        private float _time;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform>()
                .WithAll<EaseComponent>()
                .WithAll<AliveTimeComponent>()
                .Build(ref state);
            _time = 0;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public unsafe void OnUpdate(ref SystemState state)
        {
            ref BlobArray<float3> positionPathBlob = ref SystemAPI.GetSingleton<BlobPositionPathComponent>().Blob.Value.Positions;
            var positionLength = positionPathBlob.Length;
            var positions = new NativeArray<float3>(positionLength, Allocator.TempJob);
            for (int i = 0; i < positionLength; i++) positions[i] = positionPathBlob[i];
            // void* unsafePtr = positionPathBlob.GetUnsafePtr();
            // NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>(float3* unsafePtr, Allocator.TempJob);
            // ref var singleRotationBlob = ref SystemAPI.GetSingleton<BlobSingleRotationComponent>().Blob.Value;
            // ref var uniformScaleBlob = ref SystemAPI.GetSingleton<BlobUniformScaleComponent>().Blob.Value;
            // ref var stepPlanBlob = ref SystemAPI.GetSingleton<BlobStepPlanComponent>().Blob.Value;
            var time = SystemAPI.Time.DeltaTime;
            _time += time;
            var job = new EaseGroupJobChunk
            {
                DeltaTime = time,
                EaseCache = positions.AsReadOnly(),
                TimePassed = _time,
                LocalTransformHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
                EaseStepPlanHandle = SystemAPI.GetComponentTypeHandle<EaseStepPlanComponent>(),
                EaseHandle = SystemAPI.GetComponentTypeHandle<EaseComponent>(true),
                AliveTimeHandle = SystemAPI.GetComponentTypeHandle<AliveTimeComponent>(true),
            };

            state.Dependency = job.ScheduleParallel(_query, state.Dependency);
        }
    }
}