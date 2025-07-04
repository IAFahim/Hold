using System;
using Moves.Move.Data;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class SingleRotationBlobAuthoring : MonoBehaviour
    {
        public float[] degree = Array.Empty<float>();

        class Baker : Baker<SingleRotationBlobAuthoring>
        {
            public override void Bake(SingleRotationBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<SingleRotationBlob>();

                int pointCount = authoring.degree.Length;
                var arrayBuilder = builder.Allocate(ref root.Radians, pointCount);
                for (int i = 0; i < pointCount; i++) arrayBuilder[i] = math.radians(authoring.degree[i]);

                var blobRef = builder.CreateBlobAssetReference<SingleRotationBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new BlobSingleRotationComponent { Blob = blobRef });
                builder.Dispose();
            }
        }
    }
}