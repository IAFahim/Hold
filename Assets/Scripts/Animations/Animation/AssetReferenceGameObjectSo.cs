using BovineLabs.Core.ObjectManagement;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _src.Scripts.Prefabs.Prefabs.Data
{
    [CreateAssetMenu(menuName = "Game/AssetReference/GameObjectSo")]
    public class AssetReferenceGameObjectSo : ScriptableObject, IUID
    {
        [SerializeField] private int id;
        [SerializeField] private string friendlyName = string.Empty;
        [SerializeField] [UsedImplicitly] private string description = string.Empty;
        public AssetReferenceGameObject assetReferenceGameObject;

        public int ID => this.id;

        int IUID.ID
        {
            get => this.id;
            set => this.id = value;
        }

        public string FriendlyName => string.IsNullOrWhiteSpace(this.friendlyName) ? this.name : this.friendlyName;

        public string Description => this.description;
    }
}