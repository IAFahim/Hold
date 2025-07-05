using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data.Blobs
{
    public struct PositionCacheBlob
    {
        public BlobArray<float3> Positions;
    }
    public struct BlobPositionCacheComponent : IComponentData
    {
        public BlobAssetReference<PositionCacheBlob> Blob;
    }
}