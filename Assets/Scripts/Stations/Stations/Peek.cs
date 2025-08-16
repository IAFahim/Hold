using BovineLabs.Anchor;
using BovineLabs.Anchor.Toolbar;
using Unity.Entities;
using UnityEngine;

namespace Stations.Stations
{
    /// <summary> The toolbar for monitoring the number of entities, chunks and archetypes of a world. </summary>
    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    internal partial struct Peek : ISystem, ISystemStartStop
    {
        public void OnUpdate(ref SystemState state)
        {
            UnityEngine.Debug.Log("WTF");
        }

        public void OnStartRunning(ref SystemState state)
        {
        }

        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}