using Behaviors.Behavior.Data;
using BovineLabs.Core.Input;
using Lanes;
using Lanes.lanes.Data;
using Lanes.Lines.Data;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Inputs.Inputs
{
    public partial struct CharacterInputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputComponent = SystemAPI.GetSingleton<InputComponent>();
            var laneDefinition = SystemAPI.GetSingleton<LaneDefinition>();

            foreach (var (laneMover, groundMoveDirection)
                     in SystemAPI.Query<
                         RefRW<LaneMover>,
                         RefRW<GroundMoveDirectionComponent>
                     >().WithAll<PlayerTag>())
            {
                if (inputComponent.SwipeDelta.x < 0)
                {
                    laneMover.ValueRW.TargetLane = math.max(0, laneMover.ValueRO.TargetLane - 1);
                }
                else if (0 < inputComponent.SwipeDelta.x)
                {
                    laneMover.ValueRW.TargetLane =
                        math.min(laneDefinition.NumberOfLanes - 1, laneMover.ValueRO.TargetLane + 1);
                }
                else
                {
                    var currentPosition = groundMoveDirection.ValueRW.Value;
                    var targetPositionX = (laneMover.ValueRO.TargetLane - (laneDefinition.NumberOfLanes - 1) * 0.5f) *
                                          laneDefinition.LaneWidth;

                    groundMoveDirection.ValueRW.Value.x = math.lerp(currentPosition.x, targetPositionX, 0.1f);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}