// MoveWithCurve.cs

using BovineLabs.Core.Collections;
using Unity.Entities;
using Unity.Mathematics;
// Your BlobCurve namespace

namespace Move.Move.Data
{
    public struct MoveWithCurve : IComponentData
    {
        public float3 StartPosition;
        public float3 EndPosition;
        public float Duration;
        public float ElapsedTime;
        public BlobAssetReference<BlobCurve> Curve;
    }
}