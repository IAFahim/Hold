using System;
using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    [Serializable]
    public struct StationBlob: IComponentData
    {
        public BlobAssetReference<BlobArray<Station>> BlobAssetRef;
    }
}