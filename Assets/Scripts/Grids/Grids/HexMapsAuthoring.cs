using Grids.Grids;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HexMapsAuthoring : MonoBehaviour
{
    [SerializeField] private HexMapsData hexMapsData;

    public HexMapsData Data => hexMapsData;

    private class Baker : Baker<HexMapsAuthoring>
    {
        public override void Bake(HexMapsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            DependsOn(authoring.hexMapsData);
            AddComponent(entity, new HexMapsBlobRef
            {
                Ref = CreateBlobAsset(authoring.Data)
            });
        }

        private BlobAssetReference<HexMapsBlob> CreateBlobAsset(HexMapsData hexMaps)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<HexMapsBlob>();

            root.Rows = hexMaps.rows;
            root.Columns = hexMaps.columns;
            root.HexWidth = hexMaps.hexWidth;
            root.HexLength = hexMaps.hexLength;

            // Build maps array
            var mapsArray = builder.Allocate(ref root.Maps, hexMaps.layers.Count);

            for (int mapIndex = 0; mapIndex < hexMaps.layers.Count; mapIndex++)
            {
                var layer = hexMaps.layers[mapIndex];
                if (layer == null) continue;

                var rowsArray = builder.Allocate(ref mapsArray[mapIndex], hexMaps.rows);

                for (int row = 0; row < hexMaps.rows; row++)
                {
                    var colsArray = builder.Allocate(ref rowsArray[row], hexMaps.columns);
                    for (int col = 0; col < hexMaps.columns; col++)
                    {
                        colsArray[col] = layer.GetHeight(row, col, hexMaps.columns);
                    }
                }
            }

            var result = builder.CreateBlobAssetReference<HexMapsBlob>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}
