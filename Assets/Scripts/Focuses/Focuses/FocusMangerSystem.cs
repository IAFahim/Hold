using Focuses.Focuses.Data;
using Unity.Burst;
using Unity.Entities;

namespace Focuses.Focuses
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct FocusMangerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int highestPriority = int.MinValue;
            var highetPriorityEntity = Entity.Null;
            foreach (
                var (focusComponent, entity) in
                SystemAPI.Query<RefRO<FocusPriorityComponent>>().WithEntityAccess()
            )
            {
                if (focusComponent.ValueRO.Priority <= highestPriority) continue;
                highestPriority = focusComponent.ValueRO.Priority;
                highetPriorityEntity = entity;
            }

            SystemAPI.GetSingletonRW<FocusSingletonComponent>().ValueRW.Entity = highetPriorityEntity;
        }
    }
}