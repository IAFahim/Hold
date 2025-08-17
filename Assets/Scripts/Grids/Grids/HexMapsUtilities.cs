using Unity.Mathematics;
using UnityEngine;

public static class HexMapsUtilities
{
    public static float3 GetHexPosition(int row, int col, float hexWidth, float hexLength)
    {
        float x = col * hexWidth * 0.75f;
        float z = row * hexLength;

        // Offset odd columns
        if (col % 2 == 1)
        {
            z += hexLength * 0.5f;
        }

        return new float3(x, 0, z);
    }

    public static void ImportFromCSV(HexMapsData data, string csvContent, int layerIndex = 0)
    {
        if (layerIndex >= data.layers.Count || data.layers[layerIndex] == null) return;

        var lines = csvContent.Split('\n');
        var layer = data.layers[layerIndex];

        for (int row = 0; row < math.min(lines.Length, data.rows); row++)
        {
            var values = lines[row].Split(',');
            for (int col = 0; col < math.min(values.Length, data.columns); col++)
            {
                if (sbyte.TryParse(values[col].Trim(), out sbyte value))
                {
                    layer.SetHeight(row, col, data.columns, value);
                }
            }
        }
    }

    public static string ExportToCSV(HexMapsData data, int layerIndex = 0)
    {
        if (layerIndex >= data.layers.Count || data.layers[layerIndex] == null) return "";

        var layer = data.layers[layerIndex];
        var csv = new System.Text.StringBuilder();

        for (int row = 0; row < data.rows; row++)
        {
            for (int col = 0; col < data.columns; col++)
            {
                if (col > 0) csv.Append(",");
                csv.Append(layer.GetHeight(row, col, data.columns));
            }

            csv.AppendLine();
        }

        return csv.ToString();
    }
}
