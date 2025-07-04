using System;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class PositionPathBlobAuthoring : MonoBehaviour
    {
        public float3[] positions = Array.Empty<float3>();

        class Baker : Baker<PositionPathBlobAuthoring>
        {
            public override void Bake(PositionPathBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<PositionPathBlob>();

                int pointCount = authoring.positions.Length;
                var arrayBuilder = builder.Allocate(ref root.Positions, pointCount);
                for (int i = 0; i < pointCount; i++) arrayBuilder[i] = authoring.positions[i];

                var blobRef = builder.CreateBlobAssetReference<PositionPathBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new BlobPositionPathComponent { Blob = blobRef });
                builder.Dispose();
            }
        }
    }
}