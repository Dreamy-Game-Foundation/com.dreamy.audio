using System.Reflection;
using UnityEngine;

namespace Dreamy.Audio.Tests
{
    internal static class TestSerialized
    {
        public static void Set<T>(T target, string fieldName, object value)
        {
            var owner = typeof(T);
            var field = FindField(owner, fieldName);
            AssertField(field, owner, fieldName);
            field.SetValue(target, value);
        }

        public static AudioBusDefinition CreateBus(string id)
        {
            var bus = new AudioBusDefinition();
            Set(bus, "id", id);
            return bus;
        }

        private static void AssertField(FieldInfo field, System.Type owner, string fieldName)
        {
            if (field == null)
            {
                throw new System.MissingFieldException(owner.FullName, fieldName);
            }
        }

        private static FieldInfo FindField(System.Type owner, string fieldName)
        {
            var type = owner;
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
