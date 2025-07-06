using System;
using Moves.Move.Data;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class PlaneRotationCacheBlobAuthoring : MonoBehaviour
    {
        public float[] degree = Array.Empty<float>();

        class Baker : Baker<PlaneRotationCacheBlobAuthoring>
        {
            public override void Bake(PlaneRotationCacheBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<PlaneRotationCacheBlob>();

                int length = authoring.degree.Length;
                var arrayBuilder = builder.Allocate(ref root.Radians, length);
                for (int i = 0; i < length; i++) arrayBuilder[i] = math.radians(authoring.degree[i]);

                var blobRef = builder.CreateBlobAssetReference<PlaneRotationCacheBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new BlobPlaneRotationCacheComponent { Blob = blobRef });
                builder.Dispose();
            }
        }
    }
}