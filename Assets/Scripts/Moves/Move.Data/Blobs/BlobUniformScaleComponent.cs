using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct UniformScaleBlob
    {
        public BlobArray<float> Scale;
    }
    
    public struct BlobUniformScaleComponent : IComponentData
    {
        public BlobAssetReference<UniformScaleBlob> Blob;
    }
}