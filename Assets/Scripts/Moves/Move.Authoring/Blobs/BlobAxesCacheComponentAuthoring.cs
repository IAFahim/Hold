using System;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class BlobAxesCacheComponentAuthoring : MonoBehaviour
    {
        public float[] axes = Array.Empty<float>();

        public class BlobAxisCacheComponentBaker : Baker<BlobAxesCacheComponentAuthoring>
        {
            public override void Bake(BlobAxesCacheComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<AxesCacheBlob>();

                int length = authoring.axes.Length;
                var arrayBuilder = builder.Allocate(ref root.Axes, length);
                for (int i = 0; i < length; i++) arrayBuilder[i] = authoring.axes[i];

                var blobRef = builder.CreateBlobAssetReference<AxesCacheBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new BlobAxesCacheComponent { Blob = blobRef });
                builder.Dispose();
            }
        }
    }
}