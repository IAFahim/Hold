using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class NameSettings : ScriptableObject, ISettings
    {
        public NameSchema[] schemas = Array.Empty<NameSchema>();
    }
}