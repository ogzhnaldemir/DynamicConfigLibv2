using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DynamicConfigLib.Core.Models
{
    public class ConfigEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public int? ConfigId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        
        public T GetTypedValue<T>()
        {
            if (string.IsNullOrEmpty(Value))
                return default;

            switch (Type.ToLower())
            {
                case "string":
                    return (T)(object)Value;
                case "int":
                case "integer":
                    return (T)(object)int.Parse(Value);
                case "bool":
                case "boolean":
                    return (T)(object)bool.Parse(Value);
                case "double":
                    return (T)(object)double.Parse(Value);
                default:
                    return (T)(object)Value;
            }
        }
    }
} 