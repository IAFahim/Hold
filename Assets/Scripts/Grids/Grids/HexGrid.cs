using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Grids.Grids
{
    [BurstCompile]
    public static class HexGridUtils
    {
        public static IEnumerable<float3> CreateGrid(int row, int column, float xOffset, float yOffset)
        {
            var positions = new List<float3>();
            var startY = GridHeight(row, yOffset);

            for (int r = 0; r < row; r++)
            {
                var y = startY - r * yOffset;
                var xPositions = GetXArray(r, column, xOffset, yOffset, r % 2 != 0);

                foreach (var x in xPositions)
                {
                    positions.Add(new float3(x, 0, y));
                }
            }

            return positions;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GridHeight(int row, float hexHeight) => row / 2f * hexHeight;

        // Calculate starting position to center the line of hex tiles
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GridWidth(int column, float xOffset, float yOffset)
        {
            return -(column - 1) * xOffset * 0.5f;
        }

        // Calculate the position of a hex tile
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetX(float xStart, int i, float offsetX) => xStart + i * offsetX;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<float> GetXArray(
            int rowIndex, int column,
            float xOffset, float yOffset,
            bool isOddRow
        )
        {
            float startX = GridWidth(column, xOffset, yOffset);
            int actualColumns = isOddRow ? column - 1 : column;

            // Offset odd rows by half the x spacing for hexagonal pattern
            if (isOddRow) startX += xOffset * 0.5f;

            var xArr = new List<float>();
            for (int i = 0; i < actualColumns; i++)
            {
                xArr.Add(GetX(startX, i, xOffset));
            }

            return xArr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject InstantiateHexTile(int id, GameObject hexPrefab, Transform parent, float3 position)
        {
            GameObject hexTile = Object.Instantiate(hexPrefab, position, Quaternion.identity, parent);
            var tileComponent = hexTile.GetComponent<HexGridTile>();
            if (tileComponent != null)
            {
                tileComponent.id = id;
                tileComponent.height = GetHeightInByte(position.y);
            }

            hexTile.name = $"HexTile {id}";
            return hexTile;
        }

        public const float HexMinHeight = -2;
        public const float HexMaxHeight = 2;

        /// <summary>
        /// Sets height from world position to normalized sbyte value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte GetHeightInByte(float worldHeight)
        {
            return (sbyte)math.remap(HexMinHeight, HexMaxHeight, sbyte.MinValue, sbyte.MaxValue, worldHeight);
        }
    }

    public class HexGrid : MonoBehaviour
    {
        [Header("Grid Settings")] public GameObject hexPrefab;
        public int row = 128;
        public int column = 8;
        public float yOffset = 0.8660254f / 2; // sqrt(3)/2 for proper hex spacing
        public float xOffset = 1.5f;

        [Header("Data")] public HexMapsData gridDatas;

        [ContextMenu("Create Grid")]
        public void Create()
        {
            ClearGrid();
            var positions = HexGridUtils.CreateGrid(row, column, xOffset, yOffset);
            CreateTiles(positions);
        }

        private void CreateTiles(IEnumerable<float3> positions)
        {
            int i = 0;
            foreach (var position in positions)
            {
                HexGridUtils.InstantiateHexTile(i, hexPrefab, transform, position);
                i++;
            }
        }

        /// <summary>
        /// Clears all existing hex tiles (useful for OnValidate)
        /// </summary>
        [ContextMenu("Clear Grid")]
        private void ClearGrid()
        {
            // Remove all children in editor
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(transform.GetChild(i).gameObject);
                else
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        private void OnValidate()
        {
            // Ensure positive values
            row = Mathf.Max(1, row);
            column = Mathf.Max(1, column);
            yOffset = Mathf.Max(0.1f, yOffset);
            xOffset = Mathf.Max(0.1f, xOffset);
        }
    }
}