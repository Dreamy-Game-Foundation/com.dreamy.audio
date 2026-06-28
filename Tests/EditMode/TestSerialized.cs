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

        public static AudioVariant CreateVariant()
        {
            var variant = new AudioVariant();
            Set(variant, "clip", AudioClip.Create("test", 32, 1, 44100, false));
            return variant;
        }

        public static AudioEventDefinition CreateEvent(string key, AudioBusId bus, AudioVariant variant)
        {
            var audioEvent = new AudioEventDefinition();
            Set(audioEvent, "key", key);
            Set(audioEvent, "bus", bus.Value);
            Set(audioEvent, "variants", new System.Collections.Generic.List<AudioVariant> { variant });
            return audioEvent;
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
