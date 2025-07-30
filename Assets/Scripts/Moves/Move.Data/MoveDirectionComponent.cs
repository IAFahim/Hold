using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data
{
    public struct MoveDirectionComponent : IComponentData
    {
        public float2 GroundDirection;
    }
}