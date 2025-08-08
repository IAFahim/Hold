using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class ItemSettings : ScriptableObject, ISettings
    {
        public ItemSchema[] schemas = Array.Empty<ItemSchema>();
    }
}