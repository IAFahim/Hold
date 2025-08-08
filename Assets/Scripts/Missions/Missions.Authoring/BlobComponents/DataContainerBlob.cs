using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct DataContainerBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<DataContainer>> BlobAssetRef;
    }
}