using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Grid))]
public class HexTerrain : MonoBehaviour
{
    public enum HexOrientation { FlatTop, PointyTop }

    [Header("Grid")]
    [Min(1)] public Vector2Int size = new Vector2Int(16, 16); // width=x, height=y in Grid XY
    [Min(0.01f)] public float elevationStep = 1f;
    public int defaultElevation = 0;
    public GameObject cellPrefab;
    public bool worldStartsAtZero = true;
    public HexOrientation orientation = HexOrientation.FlatTop;

    [Header("Heightmap (Baked)")]
    [Tooltip("Signed data in R channel: -1 = hole, >=0 = elevation in steps.")]
    public RenderTexture heightmapRT; // RHalf

    [SerializeField, HideInInspector] string persistentKey;

    // Persisted holes
    [SerializeField] List<Vector3Int> holes = new();
    public IReadOnlyCollection<Vector3Int> Holes => holes;

    [NonSerialized] public Dictionary<Vector3Int, HexCell> runtimeCache = new();

    public Grid Grid => GetComponent<Grid>();

    public string PersistentKey
    {
        get
        {
            if (string.IsNullOrEmpty(persistentKey))
                persistentKey = Guid.NewGuid().ToString("N");
            return persistentKey;
        }
    }

    public bool HasHole(Vector3Int c) => holes.Contains(Norm(c));
    public void AddHole(Vector3Int c) { c = Norm(c); if (!holes.Contains(c)) holes.Add(c); }
    public void RemoveHole(Vector3Int c) { holes.Remove(Norm(c)); }

    public Vector3Int Norm(Vector3Int c) => new Vector3Int(Mathf.Max(0, c.x), Mathf.Max(0, c.y), 0);

    public IEnumerable<Vector3Int> AllCoords()
    {
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                yield return new Vector3Int(x, y, 0);
    }

    void Reset() => ConfigureGrid();
    void OnValidate()
    {
        size.x = Mathf.Max(1, size.x);
        size.y = Mathf.Max(1, size.y);
        elevationStep = Mathf.Max(0.01f, elevationStep);
        ConfigureGrid();
    }

    public void ConfigureGrid()
    {
        var g = Grid;
        g.cellLayout = GridLayout.CellLayout.Hexagon;
        // XY space drives hex math; map XY onto XZ for 3D
        g.cellSwizzle = GridLayout.CellSwizzle.XZY;

        // Correct spacing for perfect hexes
        const float SQRT3_OVER_2 = 0.8660254f;
        if (orientation == HexOrientation.FlatTop)
            g.cellSize = new Vector3(SQRT3_OVER_2, 1f, 1f); // flat at top/bottom
        else
            g.cellSize = new Vector3(1f, SQRT3_OVER_2, 1f); // pointy at top/bottom

        if (worldStartsAtZero) transform.position = Vector3.zero;
    }

    // Helpers used by editor
    public (int min, int max) GetElevationRange()
    {
        bool any = false;
        int min = int.MaxValue, max = int.MinValue;
        foreach (var kv in runtimeCache)
        {
            if (!kv.Value) continue;
            any = true;
            min = Mathf.Min(min, kv.Value.elevation);
            max = Mathf.Max(max, kv.Value.elevation);
        }
        if (!any) min = max = defaultElevation;
        return (min, max);
    }
}