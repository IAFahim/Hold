// In your UI assembly (e.g., YourProject.UI)

using System;
using BovineLabs.Anchor;
using BovineLabs.Anchor.Contracts;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;

namespace BovineLabs.Sample.UI.ViewModels.Game
{
    public partial class GameViewModel: SystemObservableObject<GameViewModel.Data>
    {
        [CreateProperty(ReadOnly = true)] public int TotalDistance => this.Value.TotalDistance;
        [CreateProperty(ReadOnly = true)] public bool ExitGame => Value.ExitGame;

        [CreateProperty(ReadOnly = true)] public float Weight => this.Value.Weight;
        [CreateProperty(ReadOnly = true)] public float MaxWeight => this.Value.MaxWeight;
        
        [CreateProperty(ReadOnly = true)] public float LineSwitchSpeed => this.Value.LineSwitchSpeed;
        
        [CreateProperty(ReadOnly = true)] public float Xp => this.Value.Xp;
        [CreateProperty(ReadOnly = true)] public float XpGainRate => this.Value.XpGainRate;

        [CreateProperty(ReadOnly = true)] public float MaxHealth => this.Value.MaxHealth;
        [CreateProperty(ReadOnly = true)] public float Health => this.Value.Health;
        
        [CreateProperty(ReadOnly = true)] public float StarCount => this.Value.StarCount;
        
        [CreateProperty(ReadOnly = true)] public float Speed => this.Value.Speed;
        [CreateProperty(ReadOnly = true)] public float BoostTime => this.Value.BoostTime;
        [CreateProperty(ReadOnly = true)] public float MaxBoostTime => this.Value.MaxBoostTime;
        [CreateProperty(ReadOnly = true)] public float BoostGenerateRate => this.Value.BoostGenerateRate;


        [Serializable]
        public partial struct Data : IComponentData
        {
            [SystemProperty] [SerializeField] private int totalDistance;
            [SystemProperty] [SerializeField] private bool exitGame;

            [SystemProperty] [SerializeField] private float weight;
            [SystemProperty] [SerializeField] private float maxWeight;

            [SystemProperty] [SerializeField] private float lineSwitchSpeed;

            [SystemProperty] [SerializeField] private float xp;
            [SystemProperty] [SerializeField] private float xpGainRate;


            [SystemProperty] [SerializeField] private float health;
            [SystemProperty] [SerializeField] private float maxHealth;


            [SystemProperty] [SerializeField] private float starCount;

            [SystemProperty] [SerializeField] private float speed;
            
            [SystemProperty] [SerializeField] private float boostTime;
            [SystemProperty] [SerializeField] private float maxBoostTime;
            [SystemProperty] [SerializeField] private float boostGenerateRate;
        }
    }
}