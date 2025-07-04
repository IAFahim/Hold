using System;
using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    [Serializable]
    public struct NextDuration
    {
        public byte nextIndex;
        public float duration;
    }

    public struct StepPlan
    {
        public BlobArray<NextDuration> Jumps;
    }

    public struct BlobStepPlanComponent : IComponentData
    {
        public BlobAssetReference<StepPlan> Blob;
    }
}