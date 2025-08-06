using System;
using Unity.Entities;

namespace Data
{
    [Serializable]
    public struct StationBlob: IComponentData
    {
        public BlobAssetReference<BlobArray<Station>> BlobAssetRef;
    }
}