using System.Collections.Generic;
using BovineLabs.Core.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Move.Move.Authoring
{
    /// <summary>
    /// The root struct for a blob asset that contains an array of BlobCurves.
    /// </summary>
    public struct BlobCurveArray
    {
        public BlobArray<BlobCurve> Curves;
    }

    /// <summary>
    /// A runtime component that holds a reference to our BlobCurveArray asset.
    /// </summary>
    public struct AnimationCurveArrayComponent : IComponentData
    {
        public BlobAssetReference<BlobCurveArray> CurveArrayReference;
    }


    /// <summary>
    /// Authoring component (backer) to convert a List of AnimationCurves
    /// into a BlobAssetReference<BlobCurveArray>.
    /// </summary>
    public class AnimationCurveArrayAuthoring : MonoBehaviour
    {
        [Tooltip("The list of animation curves to be converted into a blob asset.")]
        public List<AnimationCurve> AnimationCurves;

        public class AnimationCurveArrayBaker : Baker<AnimationCurveArrayAuthoring>
        {
            public override void Bake(AnimationCurveArrayAuthoring authoring)
            {
                var curves = authoring.AnimationCurves;

                if (curves == null || curves.Count == 0)
                {
                    return;
                }

                var builder = new BlobBuilder(Allocator.Temp);

                ref var root = ref builder.ConstructRoot<BlobCurveArray>();

                var blobCurveArrayBuilder = builder.Allocate(ref root.Curves, curves.Count);

                bool hasValidCurve = false;
                for (int i = 0; i < curves.Count; i++)
                {
                    var sourceCurve = curves[i];
                    if (sourceCurve == null || sourceCurve.length == 0)
                    {
                        Debug.LogError(
                            $"AnimationCurve at index {i} on GameObject '{authoring.name}' is null or empty and cannot be baked. A default flat curve will be used.",
                            authoring);

                        var defaultKey = new Keyframe(0, 0);
                        var defaultCurve = new AnimationCurve(defaultKey);
                        BlobCurve.Construct(ref builder, ref blobCurveArrayBuilder[i], defaultCurve);
                    }
                    else
                    {
                        BlobCurve.Construct(ref builder, ref blobCurveArrayBuilder[i], sourceCurve);
                        hasValidCurve = true;
                    }
                }

                if (!hasValidCurve)
                {
                    Debug.LogWarning(
                        $"All AnimationCurves on '{authoring.name}' were invalid. No blob asset component will be added.",
                        authoring);
                    return;
                }

                var blobAssetReference = builder.CreateBlobAssetReference<BlobCurveArray>(Allocator.Persistent);

                AddBlobAsset(ref blobAssetReference, out _);

                AddComponent(GetEntity(TransformUsageFlags.None), new AnimationCurveArrayComponent
                {
                    CurveArrayReference = blobAssetReference
                });
                builder.Dispose();
            }
        }
    }
}