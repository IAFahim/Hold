using BovineLabs.Anchor;
using BovineLabs.Sample.UI.ViewModels.Game;
using Missions.Missions.Data;
using Unity.Burst;
using Unity.Entities;

namespace Missions.Missions
{
    public partial struct MissionSystem : ISystem, ISystemStartStop
    {
        private UIHelper<GameViewModel, GameViewModel.Data> ui;

        public void OnStartRunning(ref SystemState state) => this.ui.Bind();

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.ui = new UIHelper<GameViewModel, GameViewModel.Data>(ref state, ComponentType.ReadOnly<GameScreenTag>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref var binding = ref this.ui.Binding;
            binding.Gold = 10;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnStopRunning(ref SystemState state) => this.ui.Unbind();
    }
}