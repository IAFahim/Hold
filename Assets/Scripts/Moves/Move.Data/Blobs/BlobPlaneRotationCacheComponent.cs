using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct PlaneRotationCacheBlob
    {
        public BlobArray<float> Radians;
    }

    public struct BlobPlaneRotationCacheComponent : IComponentData
    {
        public BlobAssetReference<PlaneRotationCacheBlob> Blob;
    }
    
    
    
}