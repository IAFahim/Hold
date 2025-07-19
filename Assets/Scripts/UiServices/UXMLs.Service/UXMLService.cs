using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace _src.Scripts.UiServices.UXMLs.Service
{
    [UsedImplicitly]
    public class UxmlService : IUxmlService
    {
        private readonly Dictionary<string, VisualTreeAsset> assets;

        public UxmlService()
        {
            var uxmls = Object.FindAnyObjectByType<UxmlServiceBehaviour>()?.Values ?? Array.Empty<UxmlServiceBehaviour.KeyUXML>();
            this.assets = uxmls.ToDictionary(t => t.Key, t => t.Asset);
        }

        public VisualTreeAsset GetAsset(string assetName)
        {
            try
            {
                return this.assets[assetName];
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Can't find key {assetName}");
                throw;
            }
        }
    }
}