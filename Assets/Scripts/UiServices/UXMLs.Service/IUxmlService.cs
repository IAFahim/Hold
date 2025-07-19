using UnityEngine.UIElements;

namespace _src.Scripts.UiServices.UXMLs.Service
{
    public interface IUxmlService
    {
        VisualTreeAsset GetAsset(string assetName);
    }
}