using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data
{
    public struct MoveStartComponent : IComponentData
    {
        public float3 Value;
    }
}
