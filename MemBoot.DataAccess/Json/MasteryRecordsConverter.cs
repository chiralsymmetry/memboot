using MemBoot.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MemBoot.DataAccess.Json;

internal class MasteryRecordsConverter : JsonConverter<IDictionary<CardType, MasteryRecord>>
{
    private readonly ICollection<CardType> cardTypes;
    private readonly ICollection<Fact> facts;

    public MasteryRecordsConverter(ICollection<CardType> cardTypes, ICollection<Fact> facts)
    {
        this.cardTypes = cardTypes;
        this.facts = facts;
    }

    public override IDictionary<CardType, MasteryRecord>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IDictionary<CardType, MasteryRecord>? output = new Dictionary<CardType, MasteryRecord>();

        var data = JsonSerializer.Deserialize<IDictionary<Guid, IDictionary<Guid, double>>>(ref reader, options) ?? new Dictionary<Guid, IDictionary<Guid, double>>();
        foreach (var kvp in data)
        {
            var cardTypeGuid = kvp.Key;
            var factGuidsToMastery = kvp.Value;
            var record = new MasteryRecord();
            var cardType = cardTypes.FirstOrDefault(ct => ct != null && ct.Id == cardTypeGuid, null);
            if (cardType == null)
            {
                continue;
            }
            foreach (var innerKvp in factGuidsToMastery)
            {
                var factGuid = innerKvp.Key;
                var mastery = innerKvp.Value;
                var fact = facts.FirstOrDefault(f => f != null && f.Id == factGuid, null);
                if (fact != null)
                {
                    record[fact] = mastery;
                }
            }
            if (record.Count > 0)
            {
                output.Add(cardType, record);
            }
        }

        return output;
    }

    public override void Write(Utf8JsonWriter writer, IDictionary<CardType, MasteryRecord> masteryRecords, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var cardType in masteryRecords.Keys)
        {
            var cardTypeId = JsonSerializer.Serialize(cardType.Id, options).Trim('"');
            writer.WritePropertyName(cardTypeId);

            writer.WriteStartObject();

            var record = masteryRecords[cardType];
            foreach (var kvp in record)
            {
                var factId = JsonSerializer.Serialize(kvp.Key.Id, options).Trim('"');
                var mastery = kvp.Value;
                writer.WriteNumber(factId, mastery);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}