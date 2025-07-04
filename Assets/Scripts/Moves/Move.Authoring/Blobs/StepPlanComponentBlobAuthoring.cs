using System;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Authoring.Blobs
{
    public class StepPlanComponentBlobAuthoring : MonoBehaviour
    {
        public NextDuration[] jumps = Array.Empty<NextDuration>();

        class Baker : Baker<StepPlanComponentBlobAuthoring>
        {
            public override void Bake(StepPlanComponentBlobAuthoring blobAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<StepPlan>();

                int pointCount = blobAuthoring.jumps.Length;
                var arrayBuilder = builder.Allocate(ref root.Jumps, pointCount);
                for (int i = 0; i < pointCount; i++) arrayBuilder[i] = blobAuthoring.jumps[i];

                var blobRef = builder.CreateBlobAssetReference<StepPlan>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);
                AddComponent(entity, new BlobStepPlanComponent
                {
                    Blob = blobRef
                });
                
                builder.Dispose();
            }
        }
    }
}