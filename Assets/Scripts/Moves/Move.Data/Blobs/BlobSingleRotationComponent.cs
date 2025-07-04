using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct SingleRotationBlob
    {
        public BlobArray<float> Radians;
    }

    public struct BlobSingleRotationComponent : IComponentData
    {
        public BlobAssetReference<SingleRotationBlob> Blob;
    }
    
    
    
}