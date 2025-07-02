// MoveWithCurveAuthoring.cs

using BovineLabs.Core.Collections;
using Move.Move.Data;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

// Your BlobCurve namespace

namespace Move.Move.Authoring
{
    public class MoveWithCurveAuthoring : MonoBehaviour
    {
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Default ease-in-out
        public Transform target;
        public float duration = 2.0f;

        class Baker : Baker<MoveWithCurveAuthoring>
        {
            public override void Bake(MoveWithCurveAuthoring authoring)
            {
                // --- Input Validation ---
                if (authoring.target == null)
                {
                    Debug.LogError("MoveWithCurveAuthoring requires a Target transform.", authoring);
                    return;
                }

                if (authoring.curve == null || authoring.curve.length == 0)
                {
                    Debug.LogError("MoveWithCurveAuthoring requires a valid AnimationCurve.", authoring);
                    return;
                }

                // --- Get the Entity for this GameObject ---
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // --- Create the BlobAsset from the AnimationCurve ---
                // 1. Create a temporary BlobBuilder
                var builder = new BlobBuilder(Allocator.Temp);
            
                // 2. Construct the root BlobCurve object inside the builder
                ref var blobCurve = ref builder.ConstructRoot<BlobCurve>();
            
                // 3. Use your static constructor to populate the blob data from the AnimationCurve
                BlobCurve.Construct(ref builder, ref blobCurve, authoring.curve);
            
                // 4. Create the persistent BlobAssetReference from the builder
                var blobAssetReference = builder.CreateBlobAssetReference<BlobCurve>(Allocator.Persistent);

                // 5. IMPORTANT: Add the blob asset to the baker's dependencies.
                // This ensures the blob asset is managed correctly by the entity lifecycle.
                AddBlobAsset(ref blobAssetReference, out _);


                // --- Add the Component to the Entity ---
                AddComponent(entity, new MoveWithCurve
                {
                    StartPosition = authoring.transform.position,
                    EndPosition = authoring.target.position,
                    Duration = authoring.duration,
                    ElapsedTime = 0f,
                    Curve = blobAssetReference // Assign the created BlobAssetReference
                });
                builder.Dispose();
            }
        }
    }
}