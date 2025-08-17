using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hex Maps Data", menuName = "Hex Maps/Hex Maps Data")]
public class HexMapsData : ScriptableObject
{
    [Header("Grid Dimensions")] public int rows = 128;
    public int columns = 8;

    [Header("Hex Geometry")] public float hexWidth = 1.5f;
    public float hexLength = 0.8660254f / 2; // sqrt(3)/2

    [Header("Map Layers")] public List<HexMapLayerSO> layers = new List<HexMapLayerSO>();

    [Header("Import/Export")] public TextAsset csvImport;

    private void OnValidate()
    {
        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);
        hexWidth = Mathf.Max(0.1f, hexWidth);
        hexLength = Mathf.Max(0.1f, hexLength);

        // Ensure all layers have correct data size
        foreach (var layer in layers)
        {
            if (layer != null)
            {
                layer.ResizeData(rows, columns);
            }
        }
    }

    public void AddLayer(HexMapLayerSO newLayer)
    {
        if (newLayer != null)
        {
            newLayer.ResizeData(rows, columns);
            layers.Add(newLayer);
        }
    }

    public void RemoveLayer(int index)
    {
        if (index >= 0 && index < layers.Count)
            layers.RemoveAt(index);
    }
}