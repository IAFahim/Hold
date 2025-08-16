using UnityEngine;

[DisallowMultipleComponent]
public class HexCell : MonoBehaviour
{
    [Tooltip("Grid cell coordinate (uses Grid XY; Z is always 0).")]
    public Vector3Int coord; // x,y; z=0

    [Tooltip("Elevation in steps (multiplied by HexTerrain.elevationStep to get world Y).")]
    public int elevation;


    public void Initialize(HexTerrain terrain, Vector3Int c, int elev)
    {
        coord = new Vector3Int(c.x, c.y, 0);
        elevation = elev;
        Apply(terrain, true);
    }

    public void SetElevation(HexTerrain terrain, int e, bool snapPosition = true)
    {
        elevation = e;
        Apply(terrain, snapPosition);
    }

    public void Apply(HexTerrain owner, bool snapPosition = true)
    {
        var grid = owner.Grid;
        if (!grid) return;

        if (snapPosition)
        {
            var center = grid.GetCellCenterWorld(coord);
            var y = elevation * owner.elevationStep;
            transform.position = new Vector3(center.x, y, center.z);
        }
        else
        {
            var p = transform.position;
            p.y = elevation * owner.elevationStep;
            transform.position = p;
        }
    }
}