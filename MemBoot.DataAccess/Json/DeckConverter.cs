using MemBoot.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MemBoot.DataAccess.Json
{
    internal class DeckConverter : JsonConverter<Deck>
    {
        public override Deck? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Deck? output = new();
            string factsJson = string.Empty;
            string masteryJson = string.Empty;

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
                    if (propertyName == nameof(Deck.Id))
                    {
                        output.Id = JsonSerializer.Deserialize<Guid>(ref reader, options);
                    }
                    else if (propertyName == nameof(Deck.Name))
                    {
                        var value = reader.GetString();
                        output.Name = value ?? output.Name;
                    }
                    else if (propertyName == nameof(Deck.Description))
                    {
                        var value = reader.GetString();
                        output.Description = value ?? output.Description;
                    }
                    else if (propertyName == nameof(Deck.Fields))
                    {
                        options = new JsonSerializerOptions(options)
                        {
                            Converters = { new FieldConverter() }
                        };
                        output.Fields = JsonSerializer.Deserialize<ICollection<Field>>(ref reader, options) ?? output.Fields;
                    }
                    else if (propertyName == nameof(Deck.Facts))
                    {
                        var tmp = JsonSerializer.Deserialize<ICollection<IDictionary<string, JsonElement>>>(ref reader, options);
                        factsJson = JsonSerializer.Serialize(tmp, options);
                    }
                    else if (propertyName == nameof(Deck.CardTypes))
                    {
                        options = new JsonSerializerOptions(options)
                        {
                            Converters = { new CardTypeConverter() }
                        };
                        output.CardTypes = JsonSerializer.Deserialize<ICollection<CardType>>(ref reader, options) ?? output.CardTypes;
                    }
                    else if (propertyName == nameof(Deck.Resources))
                    {
                        output.Resources = JsonSerializer.Deserialize<IDictionary<string, byte[]>>(ref reader, options) ?? output.Resources;
                    }
                    else if (propertyName == nameof(Deck.MasteryRecords))
                    {
                        var tmp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options);
                        masteryJson = JsonSerializer.Serialize(tmp, options);
                    }
                }
            }

            if (factsJson.Length > 0)
            {
                options = new JsonSerializerOptions(options)
                {
                    Converters = { new FactConverter(output.Fields) }
                };
                output.Facts = JsonSerializer.Deserialize<ICollection<Fact>>(factsJson, options) ?? output.Facts;
            }

            if (masteryJson.Length > 0)
            {
                options = new JsonSerializerOptions(options)
                {
                    Converters = { new MasteryRecordsConverter(output.CardTypes, output.Facts) }
                };
                output.MasteryRecords = JsonSerializer.Deserialize<IDictionary<CardType, MasteryRecord>>(masteryJson, options) ?? output.MasteryRecords;
            }

            return output;
        }

        public override void Write(Utf8JsonWriter writer, Deck deck, JsonSerializerOptions options)
        {
            options = new JsonSerializerOptions(options)
            {
                Converters = { new FactConverter(deck.Fields), new MasteryRecordsConverter(deck.CardTypes, deck.Facts) }
            };

            writer.WriteStartObject();

            var deckId = JsonSerializer.Serialize(deck.Id, options);
            writer.WritePropertyName(nameof(Deck.Id));
            writer.WriteRawValue(deckId, true);

            writer.WriteString(nameof(Deck.Name), deck.Name);
            writer.WriteString(nameof(Deck.Description), deck.Description);

            writer.WritePropertyName(nameof(Deck.Fields));
            writer.WriteRawValue(JsonSerializer.Serialize(deck.Fields, options));

            writer.WritePropertyName(nameof(Deck.Facts));
            writer.WriteRawValue(JsonSerializer.Serialize(deck.Facts, options));

            writer.WritePropertyName(nameof(Deck.CardTypes));
            writer.WriteRawValue(JsonSerializer.Serialize(deck.CardTypes, options));

            writer.WritePropertyName(nameof(Deck.Resources));
            writer.WriteRawValue(JsonSerializer.Serialize(deck.Resources, options));

            writer.WritePropertyName(nameof(Deck.MasteryRecords));
            writer.WriteRawValue(JsonSerializer.Serialize(deck.MasteryRecords, options));

            writer.WriteEndObject();
        }
    }
}