using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data
{
    public struct GroundMoveDirectionComponent : IComponentData
    {
        public float2 Value;
    }
}