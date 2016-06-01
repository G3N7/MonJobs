using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MonJobs.Serialization
{
    public abstract class ToObjectIdBsonSerializer<T> : AbstractClassSerializer<T> where T : class
    {
        private static readonly Type _convertibleType = typeof(IConvertible<ObjectId>);
        public abstract T CreateObjectFromObjectId(ObjectId serializedObj);

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            ObjectId value;
            switch (bsonType)
            {
                case BsonType.Undefined:
                    value = ObjectId.Empty;
                    context.Reader.ReadUndefined();
                    break;
                case BsonType.Null:
                    value = ObjectId.Empty;
                    context.Reader.ReadNull();
                    break;
                case BsonType.ObjectId:
                    value = context.Reader.ReadObjectId();
                    break;
                case BsonType.String:
                    value = new ObjectId(context.Reader.ReadString());
                    break;
                default:
                    throw new NotSupportedException("Unable to create the type " + args.NominalType.Name + " from the bson type " + bsonType + ".");
            }
            return this.CreateObjectFromObjectId(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            if (value == null)
            {
                context.Writer.WriteObjectId(ObjectId.Empty);
            }
            else
            {
                if (!_convertibleType.IsAssignableFrom(args.NominalType))
                {
                    throw new NotSupportedException("The type " + args.NominalType.Name + " must implement the " + _convertibleType.Name + " interface.");
                }
                var typedObj = (IConvertible<ObjectId>)value;
                context.Writer.WriteObjectId(typedObj.ToValueType());
            }
        }

    }
}