using Behaviors.Behavior.Data;
using Inputs.Inputs;
using Lanes.Lines.Data;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Lanes.lanes
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CharacterInputSystem))]
    public partial struct LaneMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
        }
    }
}
