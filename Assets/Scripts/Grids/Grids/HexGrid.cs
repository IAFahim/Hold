using Unity.Mathematics;
using UnityEngine;

namespace Grids.Grids
{
    public class HexGrid : MonoBehaviour
    {
        public GameObject hexPrefab;
        public int row = 256;
        public int column = 16;
        public float hexHeight = 0.8660254f / 2;
        public float midLineOffset = 0.7499999f;
        
        [ContextMenu("Create")]
        public void Create()
        {
            
            float upper = row / 2f * hexHeight;
            for (int y = 0; y < row; y++)
            {
                if (y % 2 == 0) 
                    CreateLine(column, 0, upper, y);
                else 
                    CreateLine(column, midLineOffset, upper, y);
                upper -= hexHeight;
            }
        }
        
        /// <summary>
        /// Creates a line of hex tiles spawning from center outward
        /// </summary>
        /// <param name="n">number of tiles in the line</param>
        /// <param name="xOffset">horizontal offset from center</param>
        /// <param name="y">vertical position</param>
        /// <param name="rowIndex">current row index for tile ID generation</param>
        public void CreateLine(int n, float xOffset, float y, int rowIndex)
        {
            if (n == 0) return;
            
            // Calculate spacing between hexagons (assumes unit spacing)
            float hexSpacing = 1f;
            
            // Calculate starting position to center the line
            float startX = -(n - 1) * hexSpacing * 0.5f + xOffset;
            
            for (int i = 0; i < n; i++)
            {
                float x = startX + i * hexSpacing;
                Vector3 position = new Vector3(x, 0, y);
                
                GameObject hexTile = Instantiate(hexPrefab, position, Quaternion.identity, transform);
                
                // Set up the tile component if it exists
                HexGridTile tileComponent = hexTile.GetComponent<HexGridTile>();
                if (tileComponent != null)
                {
                    tileComponent.id = rowIndex * column + i;
                    tileComponent.height = 0; // Default height, you can modify this as needed
                }
                
                // Name the tile for easier debugging
                hexTile.name = $"HexTile_R{rowIndex}_C{i}";
            }
        }
        
        /// <summary>
        /// Clears all existing hex tiles (useful for OnValidate)
        /// </summary>
        [ContextMenu("Clear")]
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
    }
}