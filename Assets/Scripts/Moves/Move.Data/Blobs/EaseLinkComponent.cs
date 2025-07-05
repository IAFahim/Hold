using Unity.Entities;

namespace Moves.Move.Data.Blobs
{
    public struct EaseLinkComponent : IComponentData
    {
        public byte Current;
        public float ElapsedTime;
    }
}