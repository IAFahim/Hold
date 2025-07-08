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
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new EaseGroupJobChunk
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel();
        }

    }
}