using System;
using Move.Move.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Move.Move.Authoring
{
    public class MoveWithCurveAuthoring : MonoBehaviour
    {
        public float3 startPosition;
        public float3 endPosition;
        public float duration = 1;
        public float elapsedTime;
        public EEase ease;
        public EWrapMode wrapMode = EWrapMode.Clamp; 
        

        public class MoveWithCurveBaker : Baker<MoveWithCurveAuthoring>
        {
            public override void Bake(MoveWithCurveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new MoveWithCurve
                    {
                        StartPosition = authoring.startPosition,
                        EndPosition = authoring.endPosition,
                        Duration = authoring.duration,
                        ElapsedTime = authoring.elapsedTime,
                        Ease = Ease.ToEase(authoring.ease, authoring.wrapMode)
                    });
            }
        }
    }
}