using System;
using System.Collections.Generic;
using System.Linq;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Settings;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Missions.Missions.Authoring.Editor.Graph
{
    internal abstract class SettingsSinkNodeBase<TSettings, TSchema> : Node
        where TSettings : UnityEngine.ScriptableObject
        where TSchema : UnityEngine.ScriptableObject
    {
        protected abstract string SettingsLabel { get; }
        protected abstract string InLabel { get; }

        protected override void OnDefineOptions(INodeOptionDefinition ctx)
        {
            ctx.AddNodeOption(SettingsLabel, typeof(TSettings), SettingsLabel);
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<TSchema>(InLabel).Build();
        }

        protected TSettings GetSettings()
        {
            var opt = GetNodeOptionByName(SettingsLabel);
            if (opt == null) return null;
            opt.TryGetValue(out TSettings s);
            return s;
        }

        protected IEnumerable<TSchema> CollectSchemas()
        {
            var inPort = GetInputPortByName(InLabel);
            var connected = new List<IPort>();
            inPort.GetConnectedPorts(connected);
            foreach (var p in connected)
            {
                var v = MissionGraph.ResolvePortValue<TSchema>(p);
                if (v != null) yield return v;
            }
        }

        public abstract void Sync();
    }

    [Serializable]
    internal class GoalSettingsSinkNode : SettingsSinkNodeBase<GoalSettings, GoalSchema>
    {
        protected override string SettingsLabel => "Goal Settings";
        protected override string InLabel => "Goal";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class MissionSettingsSinkNode : SettingsSinkNodeBase<MissionSettings, MissionSchema>
    {
        protected override string SettingsLabel => "Mission Settings";
        protected override string InLabel => "Mission";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class LocationSettingsSinkNode : SettingsSinkNodeBase<LocationSettings, LocationSchema>
    {
        protected override string SettingsLabel => "Location Settings";
        protected override string InLabel => "Location";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class RangeFloatSettingsSinkNode : SettingsSinkNodeBase<RangeFloatSettings, RangeFloatSchema>
    {
        protected override string SettingsLabel => "RangeFloat Settings";
        protected override string InLabel => "RangeFloat";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class RangeIntSettingsSinkNode : SettingsSinkNodeBase<RangeIntSettings, RangeIntSchema>
    {
        protected override string SettingsLabel => "RangeInt Settings";
        protected override string InLabel => "RangeInt";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class RewardSettingsSinkNode : SettingsSinkNodeBase<RewardSettings, RewardSchema>
    {
        protected override string SettingsLabel => "Reward Settings";
        protected override string InLabel => "Reward";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class TimeSettingsSinkNode : SettingsSinkNodeBase<TimeSettings, TimeSchema>
    {
        protected override string SettingsLabel => "Time Settings";
        protected override string InLabel => "Time";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class DataContainerSettingsSinkNode : SettingsSinkNodeBase<DataContainerSettings, DataContainerSchema>
    {
        protected override string SettingsLabel => "DataContainer Settings";
        protected override string InLabel => "DataContainer";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class NameSettingsSinkNode : SettingsSinkNodeBase<NameSettings, NameSchema>
    {
        protected override string SettingsLabel => "Name Settings";
        protected override string InLabel => "Name";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class DescriptionSettingsSinkNode : SettingsSinkNodeBase<DescriptionSettings, DescriptionSchema>
    {
        protected override string SettingsLabel => "Description Settings";
        protected override string InLabel => "Description";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }

    [Serializable]
    internal class ItemSettingsSinkNode : SettingsSinkNodeBase<ItemSettings, ItemSchema>
    {
        protected override string SettingsLabel => "Item Settings";
        protected override string InLabel => "Item";
        public override void Sync()
        {
            var s = GetSettings(); if (s == null) return;
            var arr = CollectSchemas().Distinct().ToArray();
            if (s.schemas == null || !s.schemas.SequenceEqual(arr))
            {
                s.schemas = arr; EditorUtility.SetDirty(s);
            }
        }
    }
}
