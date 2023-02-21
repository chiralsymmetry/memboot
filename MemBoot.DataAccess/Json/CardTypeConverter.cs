using System.Text.Json;
using System.Text.Json.Serialization;
using MemBoot.Core.Models;

namespace MemBoot.DataAccess.Json
{
    internal class CardTypeConverter : JsonConverter<CardType>
    {
        public override CardType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            CardType? output = null;
            var candidate = new CardType(default, string.Empty, string.Empty, string.Empty);
            bool guidSet = false;
            bool nameSet = false;
            bool questionTemplateSet = false;
            bool answerTemplateSet = false;

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
                    if (propertyName == nameof(CardType.Id))
                    {
                        candidate.Id = JsonSerializer.Deserialize<Guid>(ref reader, options);
                        guidSet = true;
                    }
                    else if (propertyName == nameof(CardType.Name))
                    {
                        var value = reader.GetString();
                        if (value != null)
                        {
                            candidate.Name = value;
                            nameSet = true;
                        }
                    }
                    else if (propertyName == nameof(CardType.QuestionTemplate))
                    {
                        var value = reader.GetString();
                        if (value != null)
                        {
                            candidate.QuestionTemplate = value;
                            questionTemplateSet = true;
                        }
                    }
                    else if (propertyName == nameof(CardType.AnswerTemplate))
                    {
                        var value = reader.GetString();
                        if (value != null)
                        {
                            candidate.AnswerTemplate = value;
                            answerTemplateSet = true;
                        }
                    }
                    else if (propertyName == nameof(CardType.Styling))
                    {
                        var value = reader.GetString();
                        if (value != null)
                        {
                            candidate.Styling = value;
                        }
                    }
                    else if (propertyName == nameof(CardType.InitialProbability))
                    {
                        candidate.InitialProbability = reader.GetDouble();
                    }
                    else if (propertyName == nameof(CardType.TransitionProbability))
                    {
                        candidate.TransitionProbability = reader.GetDouble();
                    }
                    else if (propertyName == nameof(CardType.SlippingProbability))
                    {
                        candidate.SlippingProbability = reader.GetDouble();
                    }
                    else if (propertyName == nameof(CardType.LuckyGuessProbability))
                    {
                        candidate.LuckyGuessProbability = reader.GetDouble();
                    }
                    else if (propertyName == nameof(CardType.MasteryThreshold))
                    {
                        candidate.MasteryThreshold = reader.GetDouble();
                    }
                    else if (propertyName == nameof(CardType.CompetencyThreshold))
                    {
                        candidate.CompetencyThreshold = reader.GetDouble();
                    }
                    else if (propertyName == nameof(CardType.CardsAreComposable))
                    {
                        candidate.CardsAreComposable = reader.GetBoolean();
                    }
                }
            }

            if (guidSet && nameSet && questionTemplateSet && answerTemplateSet)
            {
                output = candidate;
            }

            return output;
        }

        public override void Write(Utf8JsonWriter writer, CardType cardType, JsonSerializerOptions options)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(cardType, options));
        }
    }
}