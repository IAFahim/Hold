using UnityEngine;

namespace Animations.Animation.Data.Classes
{
    [CreateAssetMenu(menuName = "Game/AssetReference/GameObjectSoGroup")]
    public class AssetReferenceGameObjectSoGroup : ScriptableObject
    {
        public AssetReferenceGameObjectSo[] assets;
    }
}