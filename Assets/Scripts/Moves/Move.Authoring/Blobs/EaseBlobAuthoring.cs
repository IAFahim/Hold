using System;
using System.Runtime.CompilerServices;
using Eases.Ease.Data;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class EaseBlobAuthoring : MonoBehaviour
    {
        [SerializeField] private EaseCacheData[] jumps = Array.Empty<EaseCacheData>();
        [SerializeField] private float3[] positions = Array.Empty<float3>();
        [SerializeField] private float3[] rotation = Array.Empty<float3>();
        [SerializeField] private float[] scale = Array.Empty<float>();


        class Baker : Baker<EaseBlobAuthoring>
        {
            public override void Bake(EaseBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<EaseCacheBlob>();

                AddJump(authoring, ref builder, ref root);
                AddPosition(authoring, ref builder, ref root);
                AddQuaternion(authoring, ref builder, ref root);
                AddScale(authoring, ref builder, ref root);

                var blobRef = builder.CreateBlobAssetReference<EaseCacheBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);
                AddComponent(entity, new BlobEaseCacheComponent
                {
                    Blob = blobRef
                });

                builder.Dispose();
            }

            private static void AddJump(
                EaseBlobAuthoring authoring,
                ref BlobBuilder builder,
                ref EaseCacheBlob root
            )
            {
                int pointCount = authoring.jumps.Length;
                var arrayBuilder = builder.Allocate(ref root.Cache, pointCount);
                for (int i = 0; i < pointCount; i++)
                {
                    var easeCache = authoring.jumps[i].ToEaseCache();
                    arrayBuilder[i] = easeCache;
                }
            }

            private static void AddPosition(
                EaseBlobAuthoring authoring,
                ref BlobBuilder builder,
                ref EaseCacheBlob root
            )
            {
                int pointCount = authoring.positions.Length;
                var arrayBuilder = builder.Allocate(ref root.Positions, pointCount);
                for (int i = 0; i < pointCount; i++)
                {
                    arrayBuilder[i] = authoring.positions[i];
                }
            }

            private static void AddQuaternion(
                EaseBlobAuthoring authoring,
                ref BlobBuilder builder,
                ref EaseCacheBlob root
            )
            {
                int pointCount = authoring.rotation.Length;
                var arrayBuilder = builder.Allocate(ref root.Quaternion, pointCount);
                for (int i = 0; i < pointCount; i++)
                {
                    arrayBuilder[i] = quaternion.Euler(math.radians(authoring.rotation[i]));
                }
            }

            private static void AddScale(EaseBlobAuthoring authoring, ref BlobBuilder builder,
                ref EaseCacheBlob root)
            {
                int pointCount = authoring.scale.Length;
                var arrayBuilder = builder.Allocate(ref root.Scale, pointCount);
                for (int i = 0; i < pointCount; i++)
                {
                    arrayBuilder[i] = authoring.scale[i];
                }
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
                return
                    $"Ease: {ease}, Chanel: {easeChanel}, Next: {next}, Duration: {duration:F2}s ToEaseCache: {ToEaseCache().Ease.Value}";
            }
        }

        [Flags]
        private enum EaseChanel : byte
        {
            Position = 1 << 0, // 0b001
            Rotation = 1 << 1, // 0b010
            Scale = 1 << 2, // 0b100
        }
    }
}