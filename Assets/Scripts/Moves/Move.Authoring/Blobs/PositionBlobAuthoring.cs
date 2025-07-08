using System;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class PositionBlobAuthoring : MonoBehaviour
    {
        public float3[] positions = Array.Empty<float3>();

        class Baker : Baker<PositionBlobAuthoring>
        {
            public override void Bake(PositionBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<PositionBlob>();

                int pointCount = authoring.positions.Length;
                var arrayBuilder = builder.Allocate(ref root.Positions, pointCount);
                for (int i = 0; i < pointCount; i++) arrayBuilder[i] = authoring.positions[i];

                var blobRef = builder.CreateBlobAssetReference<PositionBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new BlobPositionComponent { Blob = blobRef });
                builder.Dispose();
            }
        }
    }
}