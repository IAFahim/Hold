using System;
using System.IO;
using System.Linq;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Scriptable;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Missions.Missions.Authoring.Editor.Graph
{
    internal interface ISchemaCreatorNode
    {
        void CreateOrUpdateAsset();
    }

    [Serializable]
    internal abstract class SchemaCreateNodeBase<TSchema> : Node, ISchemaEvaluatorNode<TSchema>, ISchemaCreatorNode
        where TSchema : ScriptableObject
    {
        // Common node option names
        protected const string OptFolder = "Assets/Settings/";
        protected const string OptFileName = "Schema";
        protected const string OptId = "ID";

        [SerializeField] protected TSchema createdAsset;

        protected abstract string DefaultFolder { get; }
        protected virtual string DefaultFileName => typeof(TSchema).Name;

        protected virtual void DefineCustomOptions(INodeOptionDefinition ctx) {}
        protected virtual void DefineCustomPorts(IPortDefinitionContext ctx) {}
        protected virtual void ApplyCustomFields(TSchema asset) {}

        protected override void OnDefineOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption(OptFileName, typeof(string), OptFileName, defaultValue: DefaultFileName);
            DefineCustomOptions(ctx);
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<string>(OptFolder).WithDisplayName(OptFolder).WithDefaultValue(DefaultFolder).Build();
            // Output port to feed downstream connections
            ctx.AddOutputPort<TSchema>("Out").Build();
            DefineCustomPorts(ctx);
        }

        public TSchema EvaluateSchemaPort(IPort _) => createdAsset;

        public void CreateOrUpdateAsset()
        {
            // Read basic options
            var folderPort = GetInputPortByName(OptFolder);
            var nameOpt = GetNodeOptionByName(OptFileName);

            string folder = DefaultFolder;
            var f = MissionGraph.ResolvePortValue<string>(folderPort);
            if (!string.IsNullOrWhiteSpace(f)) folder = f.Trim();

            string fileName = DefaultFileName;
            if (nameOpt != null && nameOpt.TryGetValue(out string fn)) fileName = string.IsNullOrWhiteSpace(fn) ? DefaultFileName : Sanitize(fn);

            EnsureFolder(folder);

            if (createdAsset == null)
            {
                createdAsset = ScriptableObject.CreateInstance<TSchema>();
                string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, fileName + ".asset"));
                AssetDatabase.CreateAsset(createdAsset, path);
            }


            // Apply type-specific fields
            ApplyCustomFields(createdAsset);

            EditorUtility.SetDirty(createdAsset);
        }

        private void SetIDFromGuid()
        {
            // Apply BaseSchema ID when present
            if (createdAsset is BaseSchema baseSchema && baseSchema.ID == 0)
            {
                var assetPath = AssetDatabase.GetAssetPath(createdAsset);
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                baseSchema.ID = (ushort)guid.GetHashCode();
            }
        }

        protected static string Sanitize(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "");
            return s.Replace(' ', '_');
        }

        protected static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    // Concrete creators below

    [Serializable]
    internal class NameSchemaCreateNode : SchemaCreateNodeBase<NameSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Name/NameSchemaSchema";
        const string OptFixed32 = "Text";

        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<string>(OptFixed32, "Name", defaultValue: "");
        }
        protected override void ApplyCustomFields(NameSchema a)
        {
            string v = string.Empty;
            GetNodeOptionByName(OptFixed32)?.TryGetValue(out v);
            a.fixed32String = v ?? string.Empty;
        }
    }

    [Serializable]
    internal class RangeFloatSchemaCreateNode : SchemaCreateNodeBase<RangeFloatSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/RangeFloat/RangeFloatSchema";
        const string OptCheck = "Check"; const string OptMin = "Min"; const string OptMax = "Max";
        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<ECheckType>(OptCheck, "Check");
            ctx.AddNodeOption<float>(OptMin, "Min", defaultValue: 0f);
            ctx.AddNodeOption<float>(OptMax, "Max", defaultValue: 0f);
        }
        protected override void ApplyCustomFields(RangeFloatSchema a)
        {
            ECheckType check = default;
            float min = 0f, max = 0f;
            GetNodeOptionByName(OptCheck)?.TryGetValue(out check);
            GetNodeOptionByName(OptMin)?.TryGetValue(out min);
            GetNodeOptionByName(OptMax)?.TryGetValue(out max);
            a.checkType = check; a.min = min; a.max = max;
        }
    }

    [Serializable]
    internal class RangeIntSchemaCreateNode : SchemaCreateNodeBase<RangeIntSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/RangeInt/RangeIntSchema";
        const string OptCheck = "Check"; const string OptMin = "Min"; const string OptMax = "Max";
        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<ECheckType>(OptCheck, "Check");
            ctx.AddNodeOption<int>(OptMin, "Min", defaultValue: 0);
            ctx.AddNodeOption<int>(OptMax, "Max", defaultValue: 0);
        }
        protected override void ApplyCustomFields(RangeIntSchema a)
        {
            ECheckType check = default;
            int min = 0, max = 0;
            GetNodeOptionByName(OptCheck)?.TryGetValue(out check);
            GetNodeOptionByName(OptMin)?.TryGetValue(out min);
            GetNodeOptionByName(OptMax)?.TryGetValue(out max);
            a.checkType = check; a.min = min; a.max = max;
        }
    }

    [Serializable]
    internal class LocationSchemaCreateNode : SchemaCreateNodeBase<LocationSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Location/LocationSchema";
        const string OptPosX = "PosX"; const string OptPosY = "PosY"; const string OptPosZ = "PosZ"; const string OptRange = "Range";

        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<float>(OptPosX, "Pos X", defaultValue: 0f);
            ctx.AddNodeOption<float>(OptPosY, "Pos Y", defaultValue: 0f);
            ctx.AddNodeOption<float>(OptPosZ, "Pos Z", defaultValue: 0f);
            ctx.AddNodeOption<float>(OptRange, "Range", defaultValue: 1f);
        }
        protected override void DefineCustomPorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<NameSchema>("Name").Build();
        }
        protected override void ApplyCustomFields(LocationSchema a)
        {
            float x = 0f, y = 0f, z = 0f, r = 1f;
            var pX = GetNodeOptionByName(OptPosX); pX?.TryGetValue(out x);
            var pY = GetNodeOptionByName(OptPosY); pY?.TryGetValue(out y);
            var pZ = GetNodeOptionByName(OptPosZ); pZ?.TryGetValue(out z);
            var pr = GetNodeOptionByName(OptRange); pr?.TryGetValue(out r);
            a.position = new Unity.Mathematics.float3(x, y, z);
            a.range = r;

            var namePort = GetInputPortByName("Name");
            var name = MissionGraph.ResolvePortValue<NameSchema>(namePort);
            if (name != null) a.nameSchema = name;
        }
    }

    [Serializable]
    internal class GoalSchemaCreateNode : SchemaCreateNodeBase<GoalSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Goal/GoalSchema";
        const string OptTarget = "TargetType";
        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<ETargetType>(OptTarget, "Target Type");
        }
        protected override void DefineCustomPorts(IPortDefinitionContext ctx)
        {
            // Accept either RangeFloatSchema or RangeIntSchema; use BaseSchema port
            ctx.AddInputPort<BaseSchema>("Range").Build();
        }
        protected override void ApplyCustomFields(GoalSchema a)
        {
            ETargetType tt = default;
            GetNodeOptionByName(OptTarget)?.TryGetValue(out tt);
            a.targetType = tt;

            var rp = GetInputPortByName("Range");
            var range = MissionGraph.ResolvePortValue<BaseSchema>(rp);
            a.rangeSchema = range;
        }
    }

    [Serializable]
    internal class TimeSchemaCreateNode : SchemaCreateNodeBase<TimeSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Time/TimeSchema";
        protected override void DefineCustomPorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<BaseSchema>("CrossLink").Build();
            ctx.AddInputPort<RangeFloatSchema>("Range").Build();
        }
        protected override void ApplyCustomFields(TimeSchema a)
        {
            var cl = MissionGraph.ResolvePortValue<BaseSchema>(GetInputPortByName("CrossLink"));
            var rf = MissionGraph.ResolvePortValue<RangeFloatSchema>(GetInputPortByName("Range"));
            a.crossLink = cl;
            a.rangeFloat = rf;
        }
    }

    [Serializable]
    internal class RewardSchemaCreateNode : SchemaCreateNodeBase<RewardSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Reward/RewardSchema";
        protected override void DefineCustomPorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<BaseSchema>("CrossLink").Build();
            ctx.AddInputPort<DataContainerSchema>("DataContainer").Build();
        }
        protected override void ApplyCustomFields(RewardSchema a)
        {
            a.crossLink = MissionGraph.ResolvePortValue<BaseSchema>(GetInputPortByName("CrossLink"));
            a.dataContainer = MissionGraph.ResolvePortValue<DataContainerSchema>(GetInputPortByName("DataContainer"));
        }
    }

    [Serializable]
    internal class DataContainerSchemaCreateNode : SchemaCreateNodeBase<DataContainerSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/DataContainer/DataContainerSchema";
        const string OptTarget = "TargetType"; const string OptNum = "NumType"; const string OptVf = "ValueF"; const string OptVi = "ValueI";
        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<ETargetType>(OptTarget, "Target Type");
            ctx.AddNodeOption<ENumType>(OptNum, "Num Type");
            ctx.AddNodeOption<float>(OptVf, "Value Float", defaultValue: 0f);
            ctx.AddNodeOption<int>(OptVi, "Value Int", defaultValue: 0);
        }
        protected override void ApplyCustomFields(DataContainerSchema a)
        {
            ETargetType target = default; ENumType num = default; float vf = 0f; int vi = 0;
            GetNodeOptionByName(OptTarget)?.TryGetValue(out target);
            GetNodeOptionByName(OptNum)?.TryGetValue(out num);
            GetNodeOptionByName(OptVf)?.TryGetValue(out vf);
            GetNodeOptionByName(OptVi)?.TryGetValue(out vi);
            a.targetType = target; a.numType = num; a.valueFloat = vf; a.valueInt = vi;
        }
    }

    [Serializable]
    internal class DescriptionSchemaCreateNode : SchemaCreateNodeBase<DescriptionSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Description/DescriptionSchema";
        const string OptText = "Text";
        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<string>(OptText, "Text", defaultValue: string.Empty);
        }
        protected override void ApplyCustomFields(DescriptionSchema a)
        {
            string t = string.Empty;
            GetNodeOptionByName(OptText)?.TryGetValue(out t);
            a.fixed128 = t ?? string.Empty;
        }
    }

    [Serializable]
    internal class ItemSchemaCreateNode : SchemaCreateNodeBase<ItemSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Item/ItemSchema";
        const string OptWeight = "WeightKg"; const string OptFlags = "Flags";
        protected override void DefineCustomOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption<float>(OptWeight, "Weight (kg)", defaultValue: 0f);
            ctx.AddNodeOption<ItemFlags>(OptFlags, "Flags", defaultValue: ItemFlags.None);
        }
        protected override void DefineCustomPorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<NameSchema>("Name").Build();
        }
        protected override void ApplyCustomFields(ItemSchema a)
        {
            float w = 0f; ItemFlags f = ItemFlags.None;
            GetNodeOptionByName(OptWeight)?.TryGetValue(out w);
            GetNodeOptionByName(OptFlags)?.TryGetValue(out f);
            a.weightKg = w; a.flags = f;
            var name = MissionGraph.ResolvePortValue<NameSchema>(GetInputPortByName("Name"));
            if (name != null) a.name = name.fixed32String.ToString();
        }
    }

    [Serializable]
    internal class MissionSchemaCreateNode : SchemaCreateNodeBase<MissionSchema>
    {
        protected override string DefaultFolder => "Assets/Settings/Mission/MissionSchema";
        protected override void DefineCustomPorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<NameSchema>("Fixed32String").Build();
            ctx.AddInputPort<LocationSchema>("Location").Build();
            ctx.AddInputPort<GoalSchema>("Goal").Build(); // multi-in allowed
        }
        protected override void ApplyCustomFields(MissionSchema a)
        {
            var name = MissionGraph.ResolvePortValue<NameSchema>(GetInputPortByName("Name"));
            var loc = MissionGraph.ResolvePortValue<LocationSchema>(GetInputPortByName("Location"));
            a.nameSchema = name;
            a.locationSchema = loc;

            var gp = GetInputPortByName("Goal");
            var connected = new System.Collections.Generic.List<IPort>();
            gp.GetConnectedPorts(connected);
            a.goals = connected
                .Select(MissionGraph.ResolvePortValue<GoalSchema>)
                .Where(g => g != null)
                .Distinct()
                .ToArray();
        }
    }
}
