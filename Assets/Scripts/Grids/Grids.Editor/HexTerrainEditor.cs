#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexTerrain))]
public class HexTerrainEditor : Editor
{
    enum PaintMode
    {
        Add,
        Erase,
        Raise,
        Lower,
        SetHeight
    }

    HexTerrain t;
    Grid grid;

    // UI state
    bool enableScenePainting;
    PaintMode mode = PaintMode.Add;
    int setHeight = 0;
    int brushRadius = 1;

    // Persistence to EditorPrefs
    const string PrefKeyPrefix = "HEX_TERRAIN_";

    [System.Serializable]
    class PersistData
    {
        public List<Int3> holes = new();
    }

    [System.Serializable]
    struct Int3
    {
        public int x, y, z;

        public Int3(Vector3Int v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }

    void OnEnable()
    {
        t = (HexTerrain)target;
        grid = t.Grid;
        EnsureCache();
        LoadHolesFromEditorPrefs();
    }

    void EnsureCache()
    {
        t.runtimeCache.Clear();
        foreach (var c in t.GetComponentsInChildren<HexCell>(true))
            t.runtimeCache[c.coord] = c;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Hex Terrain", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Grid", t.Grid, typeof(Grid), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("size"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("elevationStep"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultElevation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cellPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("worldStartsAtZero"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orientation"));

        EditorGUILayout.Space(6);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate / Regenerate", GUILayout.Height(24)))
            {
                LoadHolesFromEditorPrefs();
                Generate();
            }

            if (GUILayout.Button("Clear All", GUILayout.Height(24)))
            {
                ClearAllChildren();
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Scene Painting", EditorStyles.boldLabel);
        enableScenePainting = EditorGUILayout.Toggle("Enable Scene Painting", enableScenePainting);
        mode = (PaintMode)EditorGUILayout.EnumPopup("Mode", mode);
        brushRadius = EditorGUILayout.IntSlider("Brush Radius", brushRadius, 1, 20);
        if (mode == PaintMode.SetHeight)
            setHeight = EditorGUILayout.IntField("Set Height", setHeight);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Heightmap Bake (RenderTexture)", EditorStyles.boldLabel);
        t.heightmapRT =
            (RenderTexture)EditorGUILayout.ObjectField("Heightmap RT (RHalf)", t.heightmapRT, typeof(RenderTexture),
                false);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Bake → RenderTexture"))
                BakeToRenderTexture();
            if (GUILayout.Button("New RT Asset"))
                CreateNewRenderTextureAsset();
        }

        EditorGUILayout.HelpBox("R channel stores elevation (float). Holes use R = -1. Size equals grid size.",
            MessageType.Info);

        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save Holes to EditorPrefs"))
                SaveHolesToEditorPrefs();
            if (GUILayout.Button("Load Holes from EditorPrefs"))
                LoadHolesFromEditorPrefs();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (!enableScenePainting) return;
        if (!t || !grid) return;

        var e = Event.current;
        if (e.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // Hotkeys
        if (e.type == EventType.KeyDown)
        {
            bool changed = true;
            switch (e.keyCode)
            {
                case KeyCode.Alpha1: mode = PaintMode.Add; break;
                case KeyCode.Alpha2: mode = PaintMode.Erase; break;
                case KeyCode.Alpha3: mode = PaintMode.Raise; break;
                case KeyCode.Alpha4: mode = PaintMode.Lower; break;
                case KeyCode.Alpha5: mode = PaintMode.SetHeight; break;
                default: changed = false; break;
            }

            if (changed)
            {
                SceneView.RepaintAll();
                e.Use();
            }
        }

        if (e.type == EventType.ScrollWheel && e.modifiers == EventModifiers.Shift)
        {
            brushRadius = Mathf.Max(1, brushRadius + (e.delta.y > 0 ? -1 : 1));
            SceneView.RepaintAll();
            e.Use();
        }
        else if (e.type == EventType.ScrollWheel && e.modifiers == EventModifiers.Control)
        {
            setHeight += (e.delta.y > 0 ? -1 : 1);
            SceneView.RepaintAll();
            e.Use();
        }

        // Raycast to ground plane (Y=0)
        var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out var dist)) return;

        var hit = ray.origin + ray.direction * dist;
        var cell = grid.WorldToCell(hit);
        cell.z = 0;

        DrawBrushPreview(cell, brushRadius);

        bool paintBlocked = Tools.viewToolActive || e.alt;

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) &&
            e.button == 0 && !paintBlocked)
        {
            ApplyBrush(cell, e.alt);
            e.Use();
        }

        DrawOverlayGUI();
    }

    // ---------------- generation / clear ----------------
    void Generate()
    {
        t.ConfigureGrid();
        EnsureCache();

        // Remove outside bounds or holes
        var toRemove = t.runtimeCache.Keys.Where(c => !InBounds(c) || t.HasHole(c)).ToList();
        foreach (var c in toRemove)
        {
            if (t.runtimeCache.TryGetValue(c, out var cell) && cell)
                Undo.DestroyObjectImmediate(cell.gameObject);
            t.runtimeCache.Remove(c);
        }

        // Create missing cells
        foreach (var c in t.AllCoords())
        {
            if (t.HasHole(c)) continue;
            if (!t.runtimeCache.ContainsKey(c) || t.runtimeCache[c] == null)
                t.runtimeCache[c] = CreateCell(c, t.defaultElevation);
        }

        // Snap and tidy
        foreach (var kv in t.runtimeCache)
        {
            if (!kv.Value) continue;
            kv.Value.name = $"Hex_{kv.Key.x}_{kv.Key.y}";
            kv.Value.Apply(t, true);
        }
    }

    void ClearAllChildren()
    {
        Undo.IncrementCurrentGroup();
        foreach (var c in t.GetComponentsInChildren<HexCell>(true))
            Undo.DestroyObjectImmediate(c.gameObject);
        t.runtimeCache.Clear();
        // Keep holes list; they are used during regenerate
        SaveHolesToEditorPrefs();
    }

    // ---------------- painting ----------------
    void ApplyBrush(Vector3Int center, bool invert)
    {
        Undo.IncrementCurrentGroup();
        foreach (var c in CellsInRadius(center, brushRadius))
        {
            if (!InBounds(c)) continue;
            switch (mode)
            {
                case PaintMode.Add:
                    if (!invert) DoAdd(c);
                    else DoErase(c);
                    break;
                case PaintMode.Erase:
                    if (!invert) DoErase(c);
                    else DoAdd(c);
                    break;
                case PaintMode.Raise:
                    if (!invert) DoDelta(c, +1);
                    else DoDelta(c, -1);
                    break;
                case PaintMode.Lower:
                    if (!invert) DoDelta(c, -1);
                    else DoDelta(c, +1);
                    break;
                case PaintMode.SetHeight: DoSet(c, setHeight); break;
            }
        }
    }

    void DoAdd(Vector3Int c)
    {
        t.RemoveHole(c);
        if (!t.runtimeCache.TryGetValue(c, out var cell) || !cell)
        {
            var newCell = CreateCell(c, t.defaultElevation);
            t.runtimeCache[c] = newCell;
        }
    }

    void DoErase(Vector3Int c)
    {
        t.AddHole(c);
        if (t.runtimeCache.TryGetValue(c, out var cell) && cell)
        {
            Undo.DestroyObjectImmediate(cell.gameObject);
            t.runtimeCache.Remove(c);
        }
    }

    void DoDelta(Vector3Int c, int delta)
    {
        if (!t.runtimeCache.TryGetValue(c, out var cell) || !cell)
        {
            DoAdd(c);
            cell = t.runtimeCache[c];
        }

        Undo.RecordObject(cell, "Hex Elevation");
        cell.SetElevation(t, cell.elevation + delta);
        EditorUtility.SetDirty(cell);
    }

    void DoSet(Vector3Int c, int h)
    {
        if (!t.runtimeCache.TryGetValue(c, out var cell) || !cell)
        {
            DoAdd(c);
            cell = t.runtimeCache[c];
        }

        Undo.RecordObject(cell, "Hex Elevation");
        cell.SetElevation(t, h);
        EditorUtility.SetDirty(cell);
    }

    HexCell CreateCell(Vector3Int coord, int elevation)
    {
        if (!t.cellPrefab)
        {
            Debug.LogError("[HexTerrain] Assign a cellPrefab.");
            return null;
        }

        GameObject go;
        if (PrefabUtility.IsPartOfPrefabAsset(t.cellPrefab))
            go = (GameObject)PrefabUtility.InstantiatePrefab(t.cellPrefab, t.transform);
        else
            go = Object.Instantiate(t.cellPrefab, t.transform);

        go.name = $"Hex_{coord.x}_{coord.y}";
        Undo.RegisterCreatedObjectUndo(go, "Create Hex");

        var cell = go.GetComponent<HexCell>();
        if (!cell) cell = go.AddComponent<HexCell>();
        cell.Initialize(t, coord, elevation);
        return cell;
    }

    // ---------------- brush preview and GUI ----------------
    void DrawBrushPreview(Vector3Int center, int radius)
    {
        Handles.color = new Color(1f, 0.75f, 0.2f, 0.9f);
        foreach (var c in CellsInRadius(center, radius))
        {
            if (!InBounds(c)) continue;
            var p = grid.GetCellCenterWorld(c);
            Handles.DrawWireDisc(new Vector3(p.x, 0, p.z), Vector3.up, 0.45f * grid.cellSize.x);
        }
    }

    void DrawOverlayGUI()
    {
        Handles.BeginGUI();
        var r = new Rect(12, 12, 300, 140);
        GUILayout.BeginArea(r, EditorStyles.helpBox);
        GUILayout.Label("Hex Terrain Painter", EditorStyles.boldLabel);
        mode = (PaintMode)EditorGUILayout.EnumPopup("Mode", mode);
        brushRadius = EditorGUILayout.IntSlider("Radius", brushRadius, 1, 20);
        if (mode == PaintMode.SetHeight)
            setHeight = EditorGUILayout.IntField("Set Height", setHeight);
        GUILayout.Label("Hotkeys: 1..5 modes • Shift+Scroll radius • Ctrl+Scroll height • Alt invert");
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    // ---------------- hex math ----------------
    bool InBounds(Vector3Int c) =>
        c.x >= 0 && c.y >= 0 && c.x < t.size.x && c.y < t.size.y;

    IEnumerable<Vector3Int> CellsInRadius(Vector3Int center, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        for (int dy = -radius; dy <= radius; dy++)
        {
            var c = new Vector3Int(center.x + dx, center.y + dy, 0);
            if (HexDistance(center, c) <= radius) yield return c;
        }
    }

    int HexDistance(Vector3Int a, Vector3Int b)
    {
        // Convert Unity Grid XY offset coords to cube coords depending on orientation
        int aq, ar, bq, br;
        if (t.orientation == HexTerrain.HexOrientation.FlatTop)
        {
            // even-r horizontal layout (flat-top)
            aq = a.x - (a.y >> 1);
            ar = a.y;
            bq = b.x - (b.y >> 1);
            br = b.y;
        }
        else
        {
            // even-q vertical layout (pointy-top)
            aq = a.x;
            ar = a.y - (a.x >> 1);
            bq = b.x;
            br = b.y - (b.x >> 1);
        }

        int ax = aq, az = ar, ay = -ax - az;
        int bx = bq, bz = br, by = -bx - bz;
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }

    // ---------------- RenderTexture baking ----------------
    void CreateNewRenderTextureAsset()
    {
        var path = EditorUtility.SaveFilePanelInProject("Create Heightmap RT", $"{t.name}_heightmap.asset", "asset",
            "Select a folder and name for the RenderTexture asset.");
        if (string.IsNullOrEmpty(path)) return;

        var rt = new RenderTexture(t.size.x, t.size.y, 0, RenderTextureFormat.RHalf)
        {
            name = $"{t.name}_heightmap",
            enableRandomWrite = false,
            useMipMap = false,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0,
            autoGenerateMips = false
        };
        rt.Create();
        AssetDatabase.CreateAsset(rt, path);
        AssetDatabase.SaveAssets();
        t.heightmapRT = rt;
        EditorUtility.SetDirty(t);
        Debug.Log($"[HexTerrain] Created RenderTexture asset at: {path}");
    }

    void BakeToRenderTexture()
    {
        if (!t.heightmapRT || t.heightmapRT.width != t.size.x || t.heightmapRT.height != t.size.y)
        {
            Debug.LogWarning("[HexTerrain] Heightmap RT missing or wrong size. Creating a new one.");
            CreateNewRenderTextureAsset();
            if (!t.heightmapRT) return;
        }

        // Build a CPU texture with the data, then copy into RT
        var width = t.size.x;
        var height = t.size.y;
        var tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
        var pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var c = new Vector3Int(x, y, 0);
            int idx = y * width + x;

            if (t.HasHole(c) || !t.runtimeCache.TryGetValue(c, out var cell) || !cell)
            {
                pixels[idx] = new Color(-1f, 0f, 0f, 1f); // hole = -1 in R
            }
            else
            {
                pixels[idx] = new Color((float)cell.elevation, 0f, 0f, 1f); // elevation in R
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, false);

        Graphics.Blit(tex, t.heightmapRT);
        Debug.Log($"[HexTerrain] Baked heightmap to RenderTexture ({t.heightmapRT.width}x{t.heightmapRT.height}).");
    }

    // ---------------- persistence ----------------
    string PrefKey() => PrefKeyPrefix + t.PersistentKey;

    void SaveHolesToEditorPrefs()
    {
        var data = new PersistData { holes = t.Holes.Select(h => new Int3(h)).ToList() };
        EditorPrefs.SetString(PrefKey(), JsonUtility.ToJson(data));
    }

    void LoadHolesFromEditorPrefs()
    {
        var key = PrefKey();
        if (!EditorPrefs.HasKey(key)) return;
        var json = EditorPrefs.GetString(key);
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<PersistData>(json);
        if (data?.holes == null) return;

        var backing = typeof(HexTerrain)
            .GetField("holes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(t) as List<Vector3Int>;
        if (backing != null)
        {
            backing.Clear();
            foreach (var h in data.holes)
                backing.Add(new Vector3Int(h.x, h.y, 0));
            EditorUtility.SetDirty(t);
        }
    }

    // ---------------- Menu ----------------
    [MenuItem("Tools/Hex Terrain/Create Hex Terrain")]
    static void CreateMenu()
    {
        var go = new GameObject("Hex Terrain");
        var grid = go.AddComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Hexagon;
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        grid.cellSize = new Vector3(0.8660254f, 1f, 1f); // FlatTop default

        var terrain = go.AddComponent<HexTerrain>();
        terrain.orientation = HexTerrain.HexOrientation.FlatTop;
        terrain.worldStartsAtZero = true;
        go.transform.position = Vector3.zero;

        Selection.activeObject = go;
        EditorGUIUtility.PingObject(go);
    }
}
#endif