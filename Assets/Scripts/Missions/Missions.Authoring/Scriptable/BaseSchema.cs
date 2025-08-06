using BovineLabs.Core.ObjectManagement;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public abstract class BaseSchema : ScriptableObject, IUID
    {
        [SerializeField] private ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }
    }
}