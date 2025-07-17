#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BovineLabs.Essence.Authoring;
using UnityEditor;
using UnityEngine;

namespace StatsHelpers.StatsHelpers.Editor
{
    public abstract class EnumBuilderForStatsIntrinsic
    {
        private const string StatSettingsTypeName = "StatSettings";
        private const int GuidLength = 4;

        private class SchemaInfo
        {
            public string KeyString { get; set; }
            public string OriginalAssetName { get; set; }
            public string EnumMemberName { get; set; }
            public string DisplayName { get; set; }
            public string Guid { get; set; }
            public string AssetPath { get; set; }
            public object OriginalKey { get; set; }
            public float Factor { get; set; } // Added to store the parsed factor for intrinsics
        }

        private static StatSettings FindStatSettings()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{StatSettingsTypeName}");
            switch (guids.Length)
            {
                case 0:
                    Debug.LogError($"No '{StatSettingsTypeName}' asset found in the project.");
                    return null;
                case > 1:
                    Debug.LogWarning($"Multiple '{StatSettingsTypeName}' assets found. Using the first one: {AssetDatabase.GUIDToAssetPath(guids[0])}");
                    break;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<StatSettings>(path);
        }

        private static string SanitizeNameForEnum(string name)
        {
            string namePart = Regex.Replace(name, @"^\d+\s*", "").Trim();
            string enumMemberName = Regex.Replace(namePart, @"\s+", "");
            enumMemberName = Regex.Replace(enumMemberName, @"[^a-zA-Z0-9_]", "");

            if (string.IsNullOrEmpty(enumMemberName))
            {
                return "_InvalidName";
            }
            if (!char.IsLetter(enumMemberName[0]) && enumMemberName[0] != '_')
            {
                enumMemberName = "_" + enumMemberName;
            }
            return enumMemberName;
        }

        private static string GetDisplayName(string name)
        {
            return Regex.Replace(name, @"^\d+\s*", "").Trim();
        }

        // Helper to parse intrinsic names like "1 Health 1000"
        private static bool TryParseIntrinsicName(string name, out string displayName, out float factor)
        {
            displayName = null;
            factor = 0f;

            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                Debug.LogWarning($"Intrinsic asset name '{name}' does not follow the expected '<Key> <DisplayName> <Factor>' format. Skipping.");
                return false;
            }

            // Last part is the factor. Use InvariantCulture for robust parsing.
            if (!float.TryParse(parts[parts.Length - 1], NumberStyles.Any, CultureInfo.InvariantCulture, out factor))
            {
                Debug.LogWarning($"Could not parse factor from the last part of intrinsic asset name '{name}'. Skipping.");
                return false;
            }

            // Middle parts are the display name
            displayName = string.Join(" ", parts, 1, parts.Length - 2);
            if (string.IsNullOrWhiteSpace(displayName))
            {
                Debug.LogWarning($"Could not extract a valid display name from intrinsic asset name '{name}'. Skipping.");
                return false;
            }
            
            return true;
        }

        [MenuItem("Tools/Generate Enums/Generate EStat from StatSettings")]
        public static void GenerateStatEnum()
        {
            StatSettings settings = FindStatSettings();
            if (!settings) return;

            if (!TryGatherStatData(settings.StatSchemas, out var collectedData))
            {
                Debug.Log("No valid StatSchemaObject data found in StatSettings. No code generated for EStat.");
                return;
            }

            string generatedCode = GenerateCode("EStat", "byte", collectedData, "StatSchemaObject").ToString();
            CopyAndLog(generatedCode, "EStat");
        }

        private static bool TryGatherStatData(IReadOnlyList<StatSchemaObject> schemas, out List<SchemaInfo> collectedData)
        {
            collectedData = new List<SchemaInfo>();
            if (schemas == null || schemas.Count == 0) return false;

            Debug.Log($"Processing {schemas.Count} StatSchemaObjects from StatSettings...");
            foreach (var schema in schemas)
            {
                if (!schema) { Debug.LogWarning("Found a null StatSchemaObject. Skipping."); continue; }
                string assetPath = AssetDatabase.GetAssetPath(schema);
                if (string.IsNullOrEmpty(assetPath)) { Debug.LogWarning($"StatSchemaObject '{schema.name}' is not a persisted asset. Skipping."); continue; }
                string fullGuid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(fullGuid)) { Debug.LogWarning($"Could not get GUID for '{schema.name}'. Skipping."); continue; }

                var data = new SchemaInfo
                {
                    KeyString = schema.Key.ToString(),
                    OriginalKey = schema.Key,
                    OriginalAssetName = schema.name,
                    DisplayName = GetDisplayName(schema.name),
                    EnumMemberName = SanitizeNameForEnum(schema.name),
                    Guid = fullGuid.Length >= GuidLength ? fullGuid.Substring(0, GuidLength) : fullGuid,
                    AssetPath = assetPath
                };
                if (string.IsNullOrEmpty(data.EnumMemberName) || data.EnumMemberName == "_InvalidName") { Debug.LogWarning($"Generated invalid EnumMemberName for asset: {data.OriginalAssetName}. Skipping."); continue; }
                collectedData.Add(data);
            }
            if (collectedData.Count == 0) return false;
            collectedData = collectedData.OrderBy(s => (ushort)s.OriginalKey).ToList();
            return true;
        }

        [MenuItem("Tools/Generate Enums/Generate EIntrinsic from StatSettings")]
        public static void GenerateIntrinsicEnum()
        {
            StatSettings settings = FindStatSettings();
            if (!settings) return;

            if (!TryGatherIntrinsicData(settings.IntrinsicSchemas, out var collectedData))
            {
                Debug.Log("No valid IntrinsicSchemaObject data found. No code generated for EIntrinsic.");
                return;
            }
            string generatedCode = GenerateCode("EIntrinsic", "byte", collectedData, "IntrinsicSchemaObject").ToString();
            CopyAndLog(generatedCode, "EIntrinsic");
        }

        private static bool TryGatherIntrinsicData(IReadOnlyList<IntrinsicSchemaObject> schemas, out List<SchemaInfo> collectedData)
        {
            collectedData = new List<SchemaInfo>();
            if (schemas == null || schemas.Count == 0) return false;

            Debug.Log($"Processing {schemas.Count} IntrinsicSchemaObjects from StatSettings...");
            foreach (var schema in schemas)
            {
                if (schema == null) { Debug.LogWarning("Found a null IntrinsicSchemaObject. Skipping."); continue; }
                string assetPath = AssetDatabase.GetAssetPath(schema);
                if (string.IsNullOrEmpty(assetPath)) { Debug.LogWarning($"IntrinsicSchemaObject '{schema.name}' is not a persisted asset. Skipping."); continue; }
                string fullGuid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(fullGuid)) { Debug.LogWarning($"Could not get GUID for '{schema.name}'. Skipping."); continue; }

                if (!TryParseIntrinsicName(schema.name, out string displayName, out float factor))
                {
                    continue; // Skip if name format is incorrect
                }

                int keyValue = schema.Key;
                var data = new SchemaInfo
                {
                    KeyString = keyValue.ToString(),
                    OriginalKey = keyValue,
                    OriginalAssetName = schema.name,
                    DisplayName = displayName, // Use parsed display name
                    EnumMemberName = SanitizeNameForEnum(displayName), // Sanitize the parsed display name
                    Guid = fullGuid.Length >= GuidLength ? fullGuid.Substring(0, GuidLength) : fullGuid,
                    AssetPath = assetPath,
                    Factor = factor // Store the parsed factor
                };
                if (string.IsNullOrEmpty(data.EnumMemberName) || data.EnumMemberName == "_InvalidName") { Debug.LogWarning($"Generated invalid EnumMemberName for asset: {data.OriginalAssetName}. Skipping."); continue; }
                collectedData.Add(data);
            }
            if (collectedData.Count == 0) return false;
            collectedData = collectedData.OrderBy(s => (int)s.OriginalKey).ToList();
            return true;
        }


        // --- Common Code Generation ---
        private static StringBuilder GenerateCode(string enumName, string enumBaseType, List<SchemaInfo> collectedData, string sourceTypeName)
        {
            StringBuilder sb = new StringBuilder();
            string extClassName = $"{enumName}Ext";

            sb.AppendLine($"// Auto-generated by {nameof(EnumBuilderForStatsIntrinsic)}.cs on {System.DateTime.Now}");
            sb.AppendLine($"// Based on {sourceTypeName} assets found in StatSettings.");
            sb.AppendLine($"// Total items found: {collectedData.Count}");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using BovineLabs.Stats.Data;");
            sb.AppendLine();
            sb.AppendLine($"public enum {enumName} : {enumBaseType}");
            sb.AppendLine("{");

            HashSet<string> usedEnumNames = new HashSet<string>();
            foreach (var data in collectedData)
            {
                string finalEnumMemberName = data.EnumMemberName;
                int suffix = 1;
                while (usedEnumNames.Contains(finalEnumMemberName))
                {
                    Debug.LogWarning($"Duplicate EnumMemberName '{finalEnumMemberName}'. Appending suffix.");
                    finalEnumMemberName = $"{data.EnumMemberName}_{suffix++}";
                }
                usedEnumNames.Add(finalEnumMemberName);
                sb.AppendLine($"    {finalEnumMemberName} = {data.KeyString}, // From: {data.OriginalAssetName}, GUID Prefix: {data.Guid}");
            }

            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine($"public static class {extClassName}");
            sb.AppendLine("{");

            // GetGuid method
            sb.AppendLine($"    public static string GetGuid(this {enumName} item)");
            sb.AppendLine("    {");
            sb.AppendLine("        return item switch");
            sb.AppendLine("        {");
            foreach (var data in collectedData)
            {
                sb.AppendLine($"            {enumName}.{data.EnumMemberName} => \"{data.Guid}\",");
            }
            sb.AppendLine($"            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)");
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();

            // ToName method
            sb.AppendLine($"    public static string ToName(this {enumName} item)");
            sb.AppendLine("    {");
            sb.AppendLine("        return item switch");
            sb.AppendLine("        {");
            foreach (var data in collectedData)
            {
                sb.AppendLine($"            {enumName}.{data.EnumMemberName} => \"{data.DisplayName}\",");
            }
            sb.AppendLine($"            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)");
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();

            // FromGuid method
            sb.AppendLine($"    public static {enumName} FromGuid(string guid{GuidLength})");
            sb.AppendLine("    {");
            sb.AppendLine($"        if (guid{GuidLength} == null || guid{GuidLength}.Length != {GuidLength})");
            sb.AppendLine($"            throw new ArgumentException($\"GUID must be {GuidLength} characters long.\", nameof(guid{GuidLength}));");
            sb.AppendLine();
            sb.AppendLine($"        return guid{GuidLength} switch");
            sb.AppendLine("        {");
            var uniqueGuidEntries = collectedData.GroupBy(d => d.Guid).Select(g => g.First());
            foreach (var data in uniqueGuidEntries)
            {
                sb.AppendLine($"            \"{data.Guid}\" => {enumName}.{data.EnumMemberName},");
            }
            sb.AppendLine($"            _ => throw new ArgumentOutOfRangeException(nameof(guid{GuidLength}), guid{GuidLength}, null)");
            sb.AppendLine("        };");
            sb.AppendLine("    }");

            // Conditionally add the conversion methods
            if (enumName == "EStat")
            {
                sb.AppendLine();
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine("    public static StatKey ToKey(this EStat stat)");
                sb.AppendLine("    {");
                sb.AppendLine("        return new StatKey { Value = (ushort)stat };");
                sb.AppendLine("    }");
            }
            else if (enumName == "EIntrinsic")
            {
                // GetFactor method
                sb.AppendLine();
                sb.AppendLine($"    public static float GetFactor(this {enumName} item)");
                sb.AppendLine("    {");
                sb.AppendLine("        return item switch");
                sb.AppendLine("        {");
                foreach (var data in collectedData)
                {
                    // Use InvariantCulture to ensure dot is used as decimal separator in generated code
                    sb.AppendLine($"            {enumName}.{data.EnumMemberName} => {data.Factor.ToString(CultureInfo.InvariantCulture)}f,");
                }
                sb.AppendLine($"            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)");
                sb.AppendLine("        };");
                sb.AppendLine("    }");
                sb.AppendLine();

                // Modified ToKey method
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"    public static IntrinsicKey ToKey(this {enumName} intrinsic, out float factor)");
                sb.AppendLine("    {");
                sb.AppendLine("        factor = intrinsic.GetFactor();");
                sb.AppendLine("        return new IntrinsicKey { Value = (ushort)intrinsic };");
                sb.AppendLine("    }");
            }

            sb.AppendLine("}");
            return sb;
        }

        private static void CopyAndLog(string generatedCode, string enumName)
        {
            EditorGUIUtility.systemCopyBuffer = generatedCode;
            Debug.Log($"Generated code for {enumName} and {enumName}Ext has been copied to the clipboard. Paste it into a .cs file.");
        }
    }
}
#endif