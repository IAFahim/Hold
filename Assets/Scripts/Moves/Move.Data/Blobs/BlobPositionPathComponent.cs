using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data.Blobs
{
    public struct PositionPathBlob
    {
        public BlobArray<float3> Positions;
    }
    public struct BlobPositionPathComponent : IComponentData
    {
        public BlobAssetReference<PositionPathBlob> Blob;
    }
}