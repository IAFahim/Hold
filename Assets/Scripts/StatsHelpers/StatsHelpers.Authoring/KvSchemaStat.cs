using System;
using BovineLabs.Stats.Authoring;
using Unity.Mathematics;

namespace _src.Scripts.StatsHelpers.StatsHelpers.Authoring
{
    [Serializable]
    public struct KvSchemaStat
    {
        public StatSchemaObject schema;
        public short added;
        public half multi;
    }
}