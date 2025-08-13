using System;
using BovineLabs.Core.Authoring.Settings;

namespace Missions.Missions.Authoring.Settings
{
    public abstract class SettingsSchema<T> : SettingsBase
    {
        public T[] schemas = Array.Empty<T>();
    }
}