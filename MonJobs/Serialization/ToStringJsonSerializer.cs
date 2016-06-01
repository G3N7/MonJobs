using System;
using Newtonsoft.Json;

namespace MonJobs.Serialization
{
    public abstract class ToStringJsonSerializer<T> : JsonConverter where T : class
    {
        private static readonly Type _convertibleType = typeof(IConvertible<string>);
        private static readonly Type UType = typeof(T);

        public abstract T CreateObjectFromString(string serializedObj);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!_convertibleType.IsInstanceOfType(value))
            {
                throw new NotSupportedException("The type " + value.GetType().Name + " must implement the " + _convertibleType.Name + " interface.");
            }
            var typedObj = (IConvertible<string>)value;
            serializer.Serialize(writer, typedObj.ToValueType());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value == null ? string.Empty : serializer.Deserialize<string>(reader);
            return this.CreateObjectFromString(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return UType.IsAssignableFrom(objectType);
        }
    }
}