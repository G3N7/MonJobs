using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MonJobs.Serialization
{
    public abstract class ToStringBsonSerializer<T> : AbstractClassSerializer<T> where T : class
    {
        private static readonly Type _convertibleType = typeof(IConvertible<string>);
        public abstract T CreateObjectFromString(string serializedObj);

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            string value;
            switch (bsonType)
            {
                case BsonType.Undefined:
                    value = string.Empty;
                    context.Reader.ReadUndefined();
                    break;
                case BsonType.Null:
                    value = string.Empty;
                    context.Reader.ReadNull();
                    break;
                case BsonType.String:
                    value = context.Reader.ReadString();
                    break;
                default:
                    throw new NotSupportedException("Unable to create the type " + args.NominalType.Name + " from the the bson type " + bsonType + ".");
            }
            return this.CreateObjectFromString(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            if (value == null)
            {
                context.Writer.WriteString(string.Empty);
            }
            else
            {
                if (!_convertibleType.IsAssignableFrom(args.NominalType))
                {
                    throw new NotSupportedException("The type " + args.NominalType.Name + " must implement the " + _convertibleType.Name + " interface.");
                }
                var typedObj = (IConvertible<string>)value;
                context.Writer.WriteString(typedObj.ToValueType());
            }
        }
    }
}