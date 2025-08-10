using Missions.Missions.Authoring.Data;
using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct NameBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<Name>> BlobAssetRef;
    }
}