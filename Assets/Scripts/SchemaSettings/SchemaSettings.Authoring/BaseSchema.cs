using System.Runtime.CompilerServices;
using BovineLabs.Core.ObjectManagement;
using UnityEngine;

namespace SchemaSettings.SchemaSettings.Authoring
{
    public abstract class BaseSchema<T> : ScriptableObject, IUID where T : struct
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public abstract T ToData();
    }
}