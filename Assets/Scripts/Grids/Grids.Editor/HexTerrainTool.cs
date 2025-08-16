#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Hex Terrain Painter", typeof(HexTerrain))]
public class HexTerrainTool : EditorTool
{
    static class Styles
    {
        public static readonly GUIContent icon = EditorGUIUtility.IconContent("Grid.PaintTool", "|Hex Terrain Painter");
    }

    enum PaintMode
    {
        Add,
        Erase,
        Raise,
        Lower,
        SetHeight
    }

    public override GUIContent toolbarIcon => Styles.icon;

    HexTerrain t;
    Grid grid;

    PaintMode mode = PaintMode.Add;
    int setHeight = 0;
    int brushRadius = 1;

    public override void OnActivated()
    {
        t = (HexTerrain)target;
        grid = t ? t.Grid : null;
        Tools.hidden = true;
    }

    public override void OnWillBeDeactivated()
    {
        Tools.hidden = false;
    }


    public override void OnToolGUI(EditorWindow window)
    {
        base.OnToolGUI(window);
        if (!t || !grid) return;

        var e = Event.current;

        // Consume default controls so clicks don’t select objects while painting
        if (e.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // Hotkeys for modes
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
                window.Repaint(); // FIX: use the SceneView/EditorWindow repaint
                e.Use();
            }
        }

        // Scroll controls
        if (e.type == EventType.ScrollWheel && e.modifiers == EventModifiers.Shift)
        {
            brushRadius = Mathf.Max(1, brushRadius + (e.delta.y > 0 ? -1 : 1));
            window.Repaint();
            e.Use();
        }
        else if (e.type == EventType.ScrollWheel && e.modifiers == EventModifiers.Control)
        {
            setHeight += (e.delta.y > 0 ? -1 : 1);
            window.Repaint();
            e.Use();
        }

        // Raycast to ground plane (y=0)
        var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out var dist)) return;
        var hit = ray.origin + ray.direction * dist;
        var hoverCell = grid.WorldToCell(hit);
        hoverCell.z = 0;

        // Draw brush preview
        DrawBrushPreview(hoverCell, brushRadius);

        // Block paint while navigating the view or when Alt is held
        bool paintBlocked = Tools.viewToolActive || e.alt;

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) &&
            e.button == 0 && !paintBlocked)
        {
            ApplyBrush(hoverCell, e.alt);
            e.Use();
        }

        DrawOverlayGUI();
    }

    // ---------- painting helpers ----------
    System.Collections.Generic.IEnumerable<Vector3Int> CellsInRadius(Vector3Int center, int radius)
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
        // Even-q vertical layout conversion (fits Unity Grid hex)
        int aq = a.x, ar = a.y - (a.x >> 1);
        int bq = b.x, br = b.y - (b.x >> 1);
        int ax = aq, az = ar, ay = -ax - az;
        int bx = bq, bz = br, by = -bx - bz;
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }

    bool InBounds(Vector3Int c) =>
        c.x >= 0 && c.y >= 0 && c.x < t.size.x && c.y < t.size.y;

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
        var prefab = t.cellPrefab;
        if (!prefab)
        {
            Debug.LogError("[HexTerrain] Assign a cellPrefab on HexTerrain.");
            return null;
        }

        GameObject go;
        if (PrefabUtility.IsPartOfPrefabAsset(prefab))
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, t.transform);
        else
            go = Object.Instantiate(prefab, t.transform);

        go.name = $"Hex_{coord.x}_{coord.y}";
        Undo.RegisterCreatedObjectUndo(go, "Create Hex");

        var cell = go.GetComponent<HexCell>();
        if (!cell) cell = go.AddComponent<HexCell>();
        cell.Initialize(t, coord, elevation);
        return cell;
    }

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
        brushRadius = EditorGUILayout.IntSlider("Radius", brushRadius, 1, 12);
        if (mode == PaintMode.SetHeight)
            setHeight = EditorGUILayout.IntField("Set Height", setHeight);
        GUILayout.Label("Hotkeys: 1..5 modes, Shift+Scroll radius, Ctrl+Scroll height, Alt invert");
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}
#endif