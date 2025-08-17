// Grids/Grids/HexMaps.cs
using Unity.Collections;
using Unity.Entities;

namespace Grids.Grids
{
    // Root of the blob: map metadata + N maps, each maps[map][row][col] -> byte height
    public struct HexMapsBlob
    {
        public int Rows;
        public int Columns;

        // Hex geometry (pointy-top): width across points; length = vertical height.
        public float HexWidth;   // e.g., 1.0f
        public float HexLength;  // e.g., 0.8660254f

        public BlobArray<BlobArray<BlobArray<sbyte>>> Maps;
    }

    // Reference to the blob asset
    public struct HexMapsBlobRef : IComponentData
    {
        public BlobAssetReference<HexMapsBlob> Ref;
    }
}