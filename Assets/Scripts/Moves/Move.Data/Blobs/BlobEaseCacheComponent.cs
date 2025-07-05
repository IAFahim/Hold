using System;
using Eases.Ease.Data;
using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct EaseCache
    {
        public Ease Ease;
        public byte Next;
        public float Duration;
    }

    public struct EaseCacheBlob
    {
        public BlobArray<EaseCache> Cache;
    }

    public struct BlobEaseCacheComponent : IComponentData
    {
        public BlobAssetReference<EaseCacheBlob> Blob;
    }
}