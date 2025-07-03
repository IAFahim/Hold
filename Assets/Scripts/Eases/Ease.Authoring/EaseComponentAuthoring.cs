using Eases.Ease.Data;
using Unity.Entities;
using UnityEngine;

namespace Eases.Ease.Authoring
{
    public class EaseComponentAuthoring : MonoBehaviour
    {
        public EEase ease;
        [Range(0, 8)] public byte group;


        public class MoveWithCurveBaker : Baker<EaseComponentAuthoring>
        {
            public override void Bake(EaseComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, EaseComponent.New(authoring.ease, authoring.group));
            }
        }
    }
}