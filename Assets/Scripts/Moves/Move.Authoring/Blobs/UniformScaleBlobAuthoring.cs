using System;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class UniformScaleBlobAuthoring : MonoBehaviour
    {
        public float[] scale = Array.Empty<float>();

        class Baker : Baker<UniformScaleBlobAuthoring>
        {
            public override void Bake(UniformScaleBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<UniformScaleBlob>();

                int pointCount = authoring.scale.Length;
                var arrayBuilder = builder.Allocate(ref root.Scale, pointCount);
                for (int i = 0; i < pointCount; i++) arrayBuilder[i] = authoring.scale[i];

                var blobRef = builder.CreateBlobAssetReference<UniformScaleBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new BlobUniformScaleComponent { Blob = blobRef });
                builder.Dispose();
            }
        }
    }
}