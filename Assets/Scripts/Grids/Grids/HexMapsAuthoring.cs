// Grids/Grids/HexMapsAuthoring.cs

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Grids.Grids
{
    public class HexMapsAuthoring : MonoBehaviour
    {
        [Header("Grid size")] public int rows = 256;
        public int columns = 16;

        [Header("Hex dimensions (meters)")] public float hexWidth = 1f;
        public float hexLength = 0.8660254f; // sqrt(3)*0.5f for pointy-top with radius 0.5

        [Header("Optional: grayscale textures as height maps (0..1 -> 0..255)")]
        public float[][] heightMaps;

        [Header("If no textures, generate procedural maps")]
        public int proceduralMapCount = 3;

        public uint noiseSeed = 12345;
        public float noiseScale = 0.12f;
        public int noiseOctaves = 3;
    }

    public class HexMapsAuthoringBaker : Baker<HexMapsAuthoring>
    {
        public override void Bake(HexMapsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<HexMapsBlob>();
            root.Rows = math.max(1, authoring.rows);
            root.Columns = math.max(1, authoring.columns);
            root.HexWidth = authoring.hexWidth;
            root.HexLength = authoring.hexLength;

            int mapCount = authoring.heightMaps is { Length: > 0 }
                ? authoring.heightMaps.Length
                : math.max(1, authoring.proceduralMapCount);

            var maps = builder.Allocate(ref root.Maps, mapCount);

            for (int m = 0; m < mapCount; m++)
            {
                var rows = builder.Allocate(ref maps[m], root.Rows);
                for (int r = 0; r < root.Rows; r++)
                {
                    var cols = builder.Allocate(ref rows[r], root.Columns);
                    for (int c = 0; c < root.Columns; c++)
                    {
                        // cols[c] = SampleNoiseAsByte(
                        //     authoring.noiseSeed + (uint)m, c, r, authoring.noiseScale, authoring.noiseOctaves
                        // );
                    }
                }
            }

            var blobRef = builder.CreateBlobAssetReference<HexMapsBlob>(Allocator.Persistent);
            builder.Dispose();

            AddComponent(entity, new HexMapsBlobRef { Value = blobRef });
            AddComponent(entity, new ActiveMap { Index = 0 });
            AddComponent(entity, new MapCycleSettings
            {
                IntervalSeconds = 5f,
                Loop = true,
                Pause = false
            });
            AddComponent(entity, new MapCycleTimer { Elapsed = 0 });
        }
    }
}