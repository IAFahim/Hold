using _src.Scripts.Prefabs.Prefabs.Data;
using UnityEngine;

namespace Animations.Animation
{
    [CreateAssetMenu(menuName = "Game/AssetReference/GameObjectSoGroup")]
    public class AssetReferenceGameObjectSoGroup : ScriptableObject
    {
        public AssetReferenceGameObjectSo[] assets;
    }
}