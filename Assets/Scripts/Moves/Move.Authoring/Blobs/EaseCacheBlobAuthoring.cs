using System;
using System.Runtime.CompilerServices;
using Eases.Ease.Data;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class EaseCacheBlobAuthoring : MonoBehaviour
    {
        [SerializeField] private EaseCacheData[] jumps = Array.Empty<EaseCacheData>();

        class Baker : Baker<EaseCacheBlobAuthoring>
        {
            public override void Bake(EaseCacheBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<EaseCacheBlob>();

                int pointCount = authoring.jumps.Length;
                var arrayBuilder = builder.Allocate(ref root.Cache, pointCount);
                for (int i = 0; i < pointCount; i++)
                {
                    var easeCache = authoring.jumps[i].ToEaseCache();
                    arrayBuilder[i] = easeCache;
                }

                var blobRef = builder.CreateBlobAssetReference<EaseCacheBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);
                AddComponent(entity, new BlobEaseCacheComponent
                {
                    Blob = blobRef
                });

                builder.Dispose();
            }
        }

        [Serializable]
        private struct EaseCacheData
        {
            public EEase ease;
            public EaseChanel easeChanel;
            [Range(byte.MinValue, byte.MaxValue)] public byte next;
            public float duration;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Ease ToEase() => Ease.New(ease, (byte)easeChanel);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EaseCache ToEaseCache() => new()
            {
                Ease = ToEase(),
                Next = next,
                Duration = duration,
            };
            
            public override string ToString()
            {
                return $"Ease: {ease}, Chanel: {easeChanel}, Next: {next}, Duration: {duration:F2}s ToEaseCache: {ToEaseCache().Ease.Value}";
            }
        }

        private enum EaseChanel : byte
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            PositionXYZ = 3,
            RotationX = 4,
            RotationY = 5,
            RotationZ = 6,
            Scale = 7
        }
    }
}