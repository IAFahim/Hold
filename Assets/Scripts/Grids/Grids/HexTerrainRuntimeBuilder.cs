using UnityEngine;

[DefaultExecutionOrder(-100)]
public class HexTerrainRuntimeBuilder : MonoBehaviour
{
    public HexTerrain source;               // the HexTerrain root (with Grid, settings)
    public RenderTexture heightmapRT;       // baked RT (R = elevation, -1 = hole)
    public GameObject cellPrefabOverride;   // if null, uses source.cellPrefab

    void Awake()
    {
        if (!source || !heightmapRT) return;
        source.ConfigureGrid();

        // Readback RT once at startup
        var prev = RenderTexture.active;
        RenderTexture.active = heightmapRT;
        var w = heightmapRT.width; var h = heightmapRT.height;
        var tex = new Texture2D(w, h, TextureFormat.RGBAFloat, false, true);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;

        var pixels = tex.GetPixels();

        // Build cells
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int idx = y * w + x;
            float r = pixels[idx].r;
            if (r < 0f) continue; // hole

            var coord = new Vector3Int(x, y, 0);
            var elev = Mathf.RoundToInt(r);
            var prefab = cellPrefabOverride ? cellPrefabOverride : source.cellPrefab;
            if (!prefab) continue;

            var go = Instantiate(prefab, source.transform);
            var cell = go.GetComponent<HexCell>();
            if (!cell) cell = go.AddComponent<HexCell>();
            cell.Initialize(source, coord, elev);
        }
    }
}