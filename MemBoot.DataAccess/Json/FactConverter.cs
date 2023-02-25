using MemBoot.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MemBoot.DataAccess.Json
{
    internal class FactConverter : JsonConverter<Fact>
    {
        private readonly ICollection<Field> fields;

        public FactConverter(ICollection<Field> fields)
        {
            this.fields = fields;
        }

        public override Fact? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Fact? output = new(default, fields);

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
                    if (propertyName == nameof(Fact.Id))
                    {
                        output.Id = JsonSerializer.Deserialize<Guid>(ref reader, options);
                    }
                    else if (propertyName == nameof(Fact.FieldsContents))
                    {
                        var fieldNamesToFieldContents = JsonSerializer.Deserialize<IDictionary<string, string>>(ref reader, options) ?? new Dictionary<string, string>();
                        foreach (var kvp in fieldNamesToFieldContents)
                        {
                            var fieldName = kvp.Key;
                            var fieldContent = kvp.Value;
                            var field = fields.FirstOrDefault(f => f != null && f.Name == fieldName, null);
                            if (field != null)
                            {
                                output.FieldsContents[field] = fieldContent;
                            }
                        }
                    }
                }
            }

            return output;
        }

        public override void Write(Utf8JsonWriter writer, Fact fact, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var factId = JsonSerializer.Serialize(fact.Id, options);
            writer.WritePropertyName(nameof(Fact.Id));
            writer.WriteRawValue(factId, true);

            writer.WritePropertyName(nameof(Fact.FieldsContents));
            writer.WriteStartObject();
            foreach (var kvp in fact.FieldsContents)
            {
                writer.WriteString(kvp.Key.Name, kvp.Value);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}