using BovineLabs.Core.ObjectManagement;
using UnityEngine;

namespace Missions.Missions.Authoring.Scriptable
{
    public abstract class BaseSchema : ScriptableObject, IUID
    {
        [SerializeField] protected ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }
    }
}