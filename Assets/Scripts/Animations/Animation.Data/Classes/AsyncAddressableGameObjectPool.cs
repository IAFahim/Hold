using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Animations.Animation.Data.Classes;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Animations.Animation.Data.Classes
{
    public class AsyncAddressableGameObjectPool : MonoBehaviour
    {
        public static AsyncAddressableGameObjectPool Instance { get; private set; }

        [SerializeField] private AssetReferenceGameObjectSoGroup assetGroup;

        // Pool organized by asset index
        private readonly Dictionary<int, Queue<PooledAsset>> _availablePools = new();
        private readonly Dictionary<int, HashSet<PooledAsset>> _inUsePools = new();
        private readonly Dictionary<GameObject, PooledAsset> _objectToPooledAsset = new();

        // Track pending requests by entity
        private readonly Dictionary<Entity, PendingRequest> _pendingRequests = new();

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            ReleaseAllAssets();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Tries to get an asset immediately. If not available, starts async creation and tracks by entity.
        /// </summary>
        public bool TryGetRequest(Entity entity, int assetIndex, Vector3 position, Quaternion rotation,
            out GameObject obj)
        {
            obj = null;

            if (!IsValidAssetIndex(assetIndex))
            {
                Debug.LogError($"Invalid asset index: {assetIndex}");
                return false;
            }

            // Check if we already have a pending request for this entity
            if (_pendingRequests.TryGetValue(entity, out var existingRequest))
            {
                // Request is still pending
                if (!existingRequest.IsCompleted)
                {
                    return false;
                }

                // Request completed, return the result
                obj = existingRequest.Result;
                _pendingRequests.Remove(entity);
                return obj != null;
            }

            // Try to get from pool immediately
            if (TryGetFromPool(assetIndex, out var pooledAsset))
            {
                SetupAsset(pooledAsset, position, rotation);
                obj = pooledAsset.GameObject;
                return true;
            }

            // Start async creation and track it
            StartAsyncRequest(entity, assetIndex, position, rotation);
            return false;
        }

        /// <summary>
        /// Requests an asset. Gets from pool if available, otherwise creates new one.
        /// </summary>
        public async Task<GameObject> RequestAsset(int assetIndex, Vector3 position, Quaternion rotation)
        {
            if (!IsValidAssetIndex(assetIndex))
            {
                Debug.LogError($"Invalid asset index: {assetIndex}");
                return null;
            }

            // Try to get from pool first
            if (TryGetFromPool(assetIndex, out var pooledAsset))
            {
                SetupAsset(pooledAsset, position, rotation);
                return pooledAsset.GameObject;
            }

            // Create new asset
            return await CreateNewAsset(assetIndex, position, rotation);
        }

        /// <summary>
        /// Returns an asset to the pool for reuse
        /// </summary>
        public void ReturnAsset(GameObject gameObject)
        {
            if (!gameObject || !_objectToPooledAsset.TryGetValue(gameObject, out var pooledAsset)) return;
            ReturnToPool(pooledAsset);
        }

        /// <summary>
        /// Cancels a pending request for an entity
        /// </summary>
        public void CancelRequest(Entity entity)
        {
            _pendingRequests.Remove(entity);
        }

        #endregion

        #region Private Methods

        private bool IsValidAssetIndex(int index)
        {
            return assetGroup != null &&
                   assetGroup.assets != null &&
                   index >= 0 &&
                   index < assetGroup.assets.Length;
        }

        private bool TryGetFromPool(int assetIndex, out PooledAsset pooledAsset)
        {
            pooledAsset = null;

            if (!_availablePools.TryGetValue(assetIndex, out var queue) || queue.Count == 0)
            {
                return false;
            }

            pooledAsset = queue.Dequeue();

            // Move to in-use pool
            if (!_inUsePools.TryGetValue(assetIndex, out var inUseSet))
            {
                inUseSet = new HashSet<PooledAsset>();
                _inUsePools[assetIndex] = inUseSet;
            }

            inUseSet.Add(pooledAsset);
            return true;
        }

        private void SetupAsset(PooledAsset pooledAsset, Vector3 position, Quaternion rotation)
        {
            var transform = pooledAsset.GameObject.transform;
            transform.position = position;
            transform.rotation = rotation;
            pooledAsset.GameObject.SetActive(true);
        }

        private async Task<GameObject> CreateNewAsset(int assetIndex, Vector3 position, Quaternion rotation)
        {
            try
            {
                var assetReference = assetGroup.assets[assetIndex].assetReferenceGameObject;
                var handle = Addressables.InstantiateAsync(assetReference, position, rotation);

                var gameObject = await handle.Task;

                if (gameObject == null)
                {
                    Debug.LogError($"Failed to instantiate asset at index {assetIndex}");
                    return null;
                }

                var pooledAsset = new PooledAsset
                {
                    GameObject = gameObject,
                    AssetIndex = assetIndex,
                    Handle = handle
                };

                // Register the asset
                _objectToPooledAsset[gameObject] = pooledAsset;

                // Add to in-use pool
                if (!_inUsePools.TryGetValue(assetIndex, out var inUseSet))
                {
                    inUseSet = new HashSet<PooledAsset>();
                    _inUsePools[assetIndex] = inUseSet;
                }

                inUseSet.Add(pooledAsset);

                return gameObject;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating asset {assetIndex}: {e.Message}");
                return null;
            }
        }

        private void ReturnToPool(PooledAsset pooledAsset)
        {
            if (!pooledAsset?.GameObject) return;

            var assetIndex = pooledAsset.AssetIndex;

            // Remove from in-use pool
            if (_inUsePools.TryGetValue(assetIndex, out var inUseSet))
            {
                inUseSet.Remove(pooledAsset);
            }

            // Deactivate and add to available pool
            pooledAsset.GameObject.SetActive(false);

            if (!_availablePools.TryGetValue(assetIndex, out var availableQueue))
            {
                availableQueue = new Queue<PooledAsset>();
                _availablePools[assetIndex] = availableQueue;
            }

            availableQueue.Enqueue(pooledAsset);
        }

        private async void StartAsyncRequest(Entity entity, int assetIndex, Vector3 position, Quaternion rotation)
        {
            var pendingRequest = new PendingRequest();
            _pendingRequests[entity] = pendingRequest;

            try
            {
                var result = await CreateNewAsset(assetIndex, position, rotation);
                pendingRequest.Complete(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in async request for entity {entity}: {e.Message}");
                pendingRequest.Complete(null);
            }
        }

        private void ReleaseAllAssets()
        {
            // Release all assets
            foreach (var pooledAsset in _objectToPooledAsset.Values)
            {
                if (pooledAsset.Handle.IsValid())
                {
                    Addressables.ReleaseInstance(pooledAsset.Handle);
                }
            }

            // Clear all collections
            _availablePools.Clear();
            _inUsePools.Clear();
            _objectToPooledAsset.Clear();
            _pendingRequests.Clear();
        }

        #endregion

        #region Data Classes

        private class PooledAsset
        {
            public GameObject GameObject;
            public int AssetIndex;
            public AsyncOperationHandle<GameObject> Handle;
        }

        private class PendingRequest
        {
            public GameObject Result { get; private set; }
            public bool IsCompleted { get; private set; }

            public void Complete(GameObject result)
            {
                Result = result;
                IsCompleted = true;
            }
        }

        #endregion
    }
}