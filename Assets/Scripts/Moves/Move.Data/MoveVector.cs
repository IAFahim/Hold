using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data
{
    public struct MoveVector : IComponentData
    {
        public float2 Value;
    }
}