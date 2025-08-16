using UnityEngine;

namespace Grids.Grids
{
    public class HexGridTile : MonoBehaviour
    {
        // for editor tooling
        public int id;
        // would be normalized by dividing by byte.MaxValue
        public byte height;
        
        private void Start()
        {
            // Optional: Add any initialization logic here
        }
        
        /// <summary>
        /// Returns normalized height value (0-1)
        /// </summary>
        public float GetNormalizedHeight()
        {
            return height / (float)byte.MaxValue;
        }
        
        /// <summary>
        /// Sets height from normalized value (0-1)
        /// </summary>
        public void SetNormalizedHeight(float normalizedHeight)
        {
            height = (byte)(Mathf.Clamp01(normalizedHeight) * byte.MaxValue);
        }
    }
}