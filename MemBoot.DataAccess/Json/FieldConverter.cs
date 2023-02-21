using System.Text.Json;
using System.Text.Json.Serialization;
using MemBoot.Core.Models;

namespace MemBoot.DataAccess.Json
{
    internal class FieldConverter : JsonConverter<Field>
    {
        public override Field? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Field? field = null;
            string? fieldName = null;
            bool? fieldBool = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString() ?? "";
                    reader.Read();
                    if (propertyName == nameof(Field.Name))
                    {
                        fieldName = reader.GetString() ?? null;
                    }
                    else if (propertyName == nameof(Field.AllowHTML))
                    {
                        fieldBool = reader.GetBoolean();
                    }
                }
            }

            if (fieldName != null && fieldBool != null)
            {
                field = new Field(fieldName, fieldBool ?? false);
            }
            else if (fieldName != null)
            {
                field = new Field(fieldName);
            }

            return field;
        }

        public override void Write(Utf8JsonWriter writer, Field field, JsonSerializerOptions options)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(field, options));
        }
    }
}