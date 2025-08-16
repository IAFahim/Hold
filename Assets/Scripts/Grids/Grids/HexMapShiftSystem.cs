// Grids/Grids/HexMapShiftSystem.cs
using Unity.Burst;
using Unity.Entities;

namespace Grids.Grids
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct HexMapShiftSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HexMapsBlobRef>();
            state.RequireForUpdate<ActiveMap>();
            state.RequireForUpdate<MapCycleSettings>();
            state.RequireForUpdate<MapCycleTimer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (blobRO, settingsRW, timerRW, activeRW, heightsBuf)
                     in SystemAPI.Query<RefRO<HexMapsBlobRef>, RefRW<MapCycleSettings>, RefRW<MapCycleTimer>, RefRW<ActiveMap>, DynamicBuffer<CurrentMapHeights>>())
            {
                if (!blobRO.ValueRO.Value.IsCreated) continue;

                ref var root = ref blobRO.ValueRO.Value.Value;
                if (root.Maps.Length == 0 || settingsRW.ValueRO.Pause) continue;

                // Advance timer
                float elapsed = timerRW.ValueRO.Elapsed + dt;
                if (elapsed < settingsRW.ValueRO.IntervalSeconds)
                {
                    timerRW.ValueRW.Elapsed = elapsed;
                    continue;
                }

                // Consume one interval and advance map index
                elapsed -= settingsRW.ValueRO.IntervalSeconds;
                timerRW.ValueRW.Elapsed = elapsed;

                int nextIndex = activeRW.ValueRO.Index + 1;
                if (nextIndex >= root.Maps.Length)
                {
                    if (settingsRW.ValueRO.Loop) nextIndex = 0;
                    else
                    {
                        nextIndex = root.Maps.Length - 1;
                        settingsRW.ValueRW.Pause = true; // stop cycling at the last map
                    }
                }
                activeRW.ValueRW.Index = nextIndex;

                // Copy the selected map into the buffer (row-major)
                int total = root.Rows * root.Columns;
                if (heightsBuf.Length != total)
                    heightsBuf.ResizeUninitialized(total);
            }
        }
    }
}