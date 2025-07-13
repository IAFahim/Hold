using System;
using Unity.Cinemachine;
using UnityEngine;

namespace CinemachineLink.CinemachineLink.Data
{
    [RequireComponent(typeof(CinemachineCamera))]
    [DisallowMultipleComponent]
    public class CinemachineLinkerSingleton : MonoBehaviour
    {
        private static CinemachineLinkerSingleton _singleton;
        [SerializeField] public CinemachineCamera cinemachineCamera;
        private Transform cinemachineTargetTransform;

        public static Transform Transform => _singleton.cinemachineTargetTransform;

        private void Start()
        {
            cinemachineTargetTransform ??= new GameObject(nameof(cinemachineTargetTransform)).transform;
            cinemachineCamera.Follow = cinemachineTargetTransform;
            _singleton = this;
        }

        private void Reset() => cinemachineCamera = GetComponent<CinemachineCamera>();
    }
}