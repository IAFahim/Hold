using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct EaseStateComponent : IComponentData
    {
        public byte Current;
        public float ElapsedTime;
    }
}