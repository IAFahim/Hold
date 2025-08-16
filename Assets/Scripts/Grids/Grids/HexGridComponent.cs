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

        public BlobArray<BlobArray<BlobArray<byte>>> Maps;
    }

    // Reference to the blob asset
    public struct HexMapsBlobRef : IComponentData
    {
        public BlobAssetReference<HexMapsBlob> Value;
    }

    // Which map is currently active
    public struct ActiveMap : IComponentData
    {
        public int Index; // 0..MapCount-1
    }

    // Cycle settings + basic control
    public struct MapCycleSettings : IComponentData
    {
        public float IntervalSeconds; // default 5
        public bool Loop;             // default true
        public bool Pause;            // optional runtime control
    }

    // Per-entity timer for the cycling
    public struct MapCycleTimer : IComponentData
    {
        public float Elapsed;
    }

    // Current map’s heights (rows*columns). Faster reads for runtime systems.
    public struct CurrentMapHeights : IBufferElementData
    {
        public byte Value; // 0..255
    }
}