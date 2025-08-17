using System.Runtime.CompilerServices;
using BovineLabs.Anchor;
using BovineLabs.Sample.UI.ViewModels.Game;
using Missions.Missions.Authoring;
using Missions.Missions.Authoring.Data;
using Missions.Missions.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _src.Scripts.Missions.Missions
{
    [BurstCompile]
    public partial struct MissionSystem : ISystem
    {
        private float _distance;
        private NativeArray<Mission> _missions;
        public NativeArray<RangeFloat> _goalFloat;
        public NativeArray<RangeInt> _goalInt;
        private NativeArray<RangeFloat> _time;

        private UIHelper<GameViewModel, GameViewModel.Data> _uiHelper;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _uiHelper = new UIHelper<GameViewModel, GameViewModel.Data>(ref state, ComponentType.ReadOnly<GameScreenTag>());
        }

        public void OnStartRunning(ref SystemState state)
        {
            GatherInfoForTracking();
            _uiHelper.Bind();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GatherInfoForTracking()
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }
    }
}