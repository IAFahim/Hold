using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Grids.Grids
{
    public class HexGridTile : MonoBehaviour
    {

        // for editor tooling
        public int id;

        // would be normalized by dividing by sbyte.MaxValue
        public sbyte height;
    }
    
}