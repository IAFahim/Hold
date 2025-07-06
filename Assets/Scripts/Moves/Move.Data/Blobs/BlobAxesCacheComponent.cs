using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct BlobAxesCacheComponent : IComponentData
    {
        public BlobAssetReference<AxesCacheBlob> Blob;
    }

    public struct AxesCacheBlob
    {
        public BlobArray<float> Axes;
    }
}