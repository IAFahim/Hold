using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hex Map Layer", menuName = "Hex Maps/Hex Map Layer")]
public class HexMapLayerSO : ScriptableObject
{
    public string layerName = "New Layer";
    [TextArea(3, 10)] public string description;
    public Color layerColor = Color.white;
    [Range(sbyte.MinValue, sbyte.MaxValue)] public sbyte[] heightData = Array.Empty<sbyte>();

    public void ResizeData(int rows, int columns)
    {
        int newSize = rows * columns;
        if (heightData.Length != newSize)
        {
            Array.Resize(ref heightData, newSize);
        }
    }

    public sbyte GetHeight(int row, int col, int totalColumns)
    {
        int index = row * totalColumns + col;
        if (index >= 0 && index < heightData.Length)
            return heightData[index];
        return 0;
    }

    public void SetHeight(int row, int col, int totalColumns, sbyte height)
    {
        int index = row * totalColumns + col;
        if (index >= 0 && index < heightData.Length) heightData[index] = height;
    }
}