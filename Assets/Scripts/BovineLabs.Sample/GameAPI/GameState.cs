using BovineLabs.Core.Collections;
using BovineLabs.Core.States;
using Unity.Entities;
using Unity.Properties;

namespace BovineLabs.Sample
{
    public struct GameState : IState<BitArray256>
    {
        [CreateProperty]
        public BitArray256 Value { get; set; }
    }

    public struct GameStatePrevious : IComponentData
    {
        public BitArray256 Value;
    }
    
}