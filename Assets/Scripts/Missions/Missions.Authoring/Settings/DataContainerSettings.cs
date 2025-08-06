using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class DataContainerSettings : ScriptableObject, ISettings
    {
        public DataContainerSchema[] schemas = Array.Empty<DataContainerSchema>();
    }
}
