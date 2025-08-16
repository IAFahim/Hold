// Grids/Grids/HexGridAspect.cs
using Unity.Entities;

namespace Grids.Grids
{
    public readonly partial struct HexGridAspect : IAspect
    {
        public readonly RefRO<HexMapsBlobRef> BlobRef;
        public readonly RefRO<ActiveMap> Active;
        public readonly DynamicBuffer<CurrentMapHeights> Heights;

        public int Rows => BlobRef.ValueRO.Value.Value.Rows;
        public int Columns => BlobRef.ValueRO.Value.Value.Columns;
        public float HexWidth => BlobRef.ValueRO.Value.Value.HexWidth;
        public float HexLength => BlobRef.ValueRO.Value.Value.HexLength;
        public int MapCount => BlobRef.ValueRO.Value.Value.Maps.Length;
        public int CurrentMapIndex => Active.ValueRO.Index;

        public byte GetHeightByte(int row, int col) => Heights[row * Columns + col].Value;
        public float GetHeight01(int row, int col) => GetHeightByte(row, col) / 255f;
    }
}