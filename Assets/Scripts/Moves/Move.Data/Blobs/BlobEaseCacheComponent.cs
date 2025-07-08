using System;
using Eases.Ease.Data;
using Unity.Entities;
using Unity.Mathematics;

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
        public BlobArray<float3> Positions;
        public BlobArray<quaternion> Quaternion;
        public BlobArray<float> Scale;
    }

    public struct BlobEaseCacheComponent : IComponentData
    {
        public BlobAssetReference<EaseCacheBlob> Blob;
    }
}