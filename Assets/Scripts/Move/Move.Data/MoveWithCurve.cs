// MoveWithCurve.cs

using Unity.Entities;
using Unity.Mathematics;

namespace Move.Move.Data
{
    public struct MoveWithCurve : IComponentData
    {
        public float3 StartPosition;
        public float3 EndPosition;
        public float Duration;
        public float ElapsedTime;
        public Ease Ease;
    }
}