using System;
using BovineLabs.Stats.Authoring;
using Unity.Mathematics;

namespace StatsHelpers.StatsHelpers.Authoring
{
    [Serializable]
    public struct KvSchemaStat
    {
        public StatSchemaObject schema;
        public short added;
        public half multi;
    }
}