using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class FramerateSetter : MonoBehaviour
{
    public int Framerate = -1;
    public float FixedFramerate = 60;

    private void Start()
    {
        Application.targetFrameRate = Framerate;

        var fixedSystem =
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();
        fixedSystem.Timestep = 1f / FixedFramerate;
    }
}