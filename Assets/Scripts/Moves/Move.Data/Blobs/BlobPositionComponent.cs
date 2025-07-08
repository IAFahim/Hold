using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data.Blobs
{
    public struct PositionBlob
    {
        public BlobArray<float3> Positions;
    }
    public struct BlobPositionComponent : IComponentData
    {
        public BlobAssetReference<PositionBlob> Blob;
    }
}