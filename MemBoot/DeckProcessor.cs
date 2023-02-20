using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Media;
using System.IO;

namespace MemBoot
{
    public static class DeckProcessor
    {
        private readonly static Regex TemplateReplacement = new("{{([^}]+)}}", RegexOptions.Compiled);

        /// <summary>
        /// Returns a collection of all <see cref="Field"/>s used in a <see cref="CardType"/>'s templates. 
        /// </summary>
        private static IEnumerable<Field> FieldsUsedByCardType(Deck deck, CardType cardType)
        {
            // A CardType's question or answer template take the form of strings
            // containing "{{foo}}" where "foo" should be the name of a field.
            // So we add all field objects and remove all those with names
            // not found in the templates.
            ICollection<Field> output = new HashSet<Field>(deck.Fields);
            var template = $"{cardType.QuestionTemplate}{cardType.AnswerTemplate}";
            MatchCollection matches = TemplateReplacement.Matches(template);
            ICollection<string> fieldNames = new HashSet<string>();
            foreach (Match match in matches.Cast<Match>())
            {
                var fieldName = match.Groups[1].Value;
                fieldNames.Add(fieldName);
            }
            foreach (var field in output)
            {
                if (!fieldNames.Contains(field.Name))
                {
                    output.Remove(field);
                }
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of usable <see cref="Fact"/>s, as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="Fact"/> is considered usable for a given <see cref="CardType"/>
        /// if the <see cref="Fact"/> has contents (empty string or not) for all <see cref="Field"/>s used by the <see cref="CardType"/>.
        /// </summary>
        private static IEnumerable<Fact> UsableFacts(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            // Add all Facts, then remove all Facts not using all Fields.
            ICollection<Fact> output = new HashSet<Fact>(facts);
            var fieldsUsed = FieldsUsedByCardType(deck, cardType);
            foreach (var fact in output)
            {
                foreach (var field in fieldsUsed)
                {
                    if (!fact.FieldsContents.ContainsKey(field))
                    {
                        output.Remove(fact);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of introduced <see cref="Fact"/>s, as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="CardType"/>-<see cref="Fact"/> combination counts as introduced when it has a mastery value in the supplied <see cref="Deck"/>.
        /// </summary>
        private static IEnumerable<Fact> IntroducedFacts(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (deck.MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord record = deck.MasteryRecords[cardType];

                output = facts.Where(f => record.ContainsKey(f));
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of still unintroduced <see cref="Fact"/>s, as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="CardType"/>-<see cref="Fact"/> combination counts as unintroduced when it has no mastery value in the supplied <see cref="Deck"/>;
        /// we also disregard all <see cref="Fact"/>s not usable with this <see cref="CardType"/>.
        /// </summary>
        private static IEnumerable<Fact> UnintroducedFacts(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            // Get all usable Facts, and remove all Facts without mastery values set.
            ICollection<Fact> output = new HashSet<Fact>(UsableFacts(deck, cardType, facts));
            if (deck.MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = deck.MasteryRecords[cardType];
                foreach (var fact in results.Keys)
                {
                    output.Remove(fact);
                }
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of mastered <see cref="Fact"/>s, as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="CardType"/>-<see cref="Fact"/> combination counts as mastered when it has a mastery value
        /// in the supplied <see cref="Deck"/> not below the mastery threshold set in the <see cref="CardType"/>.
        /// </summary>
        private static IEnumerable<Fact> MasteredFacts(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (deck.MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = deck.MasteryRecords[cardType];
                output = facts.Where(f => results.ContainsKey(f) && results[f] >= cardType.MasteryThreshold);
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of still unmastered <see cref="Fact"/>s, as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="CardType"/>-<see cref="Fact"/> combination counts as unmastered when it has a mastery value
        /// in the supplied <see cref="Deck"/> below the mastery threshold set in the <see cref="CardType"/>,
        /// or if it still unintroduced.
        /// </summary>
        private static IEnumerable<Fact> UnmasteredFacts(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (deck.MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = deck.MasteryRecords[cardType];
                output = facts.Where(f => !results.ContainsKey(f) || results[f] < cardType.MasteryThreshold);
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of <see cref="Fact"/>s still at "beginner level", as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="CardType"/>-<see cref="Fact"/> combination counts as being at beginner level when it has a mastery value
        /// in the supplied <see cref="Deck"/> below the competency threshold set in the <see cref="CardType"/>,
        /// or if it still unintroduced.
        /// </summary>
        private static IEnumerable<Fact> BeginnerFacts(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (deck.MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = deck.MasteryRecords[cardType];
                output = facts.Where(f => !results.ContainsKey(f) || results[f] < cardType.CompetencyThreshold);
            }
            return output;
        }

        private static double GetMastery(Deck deck, CardType cardType, Fact fact)
        {
            double output = cardType.InitialProbability;
            if (deck.MasteryRecords.ContainsKey(cardType) && deck.MasteryRecords[cardType].ContainsKey(fact))
            {
                output = deck.MasteryRecords[cardType][fact];
            }
            return output;
        }

        private static void SetMastery(Deck deck, CardType cardType, Fact fact, double mastery)
        {
            if (!deck.MasteryRecords.ContainsKey(cardType))
            {
                deck.MasteryRecords[cardType] = new MasteryRecord();
            }
            deck.MasteryRecords[cardType][fact] = mastery;
        }

        private static void IntroduceFact(Deck deck, CardType cardType, IEnumerable<Fact> facts)
        {
            var unintroducedFacts = UnintroducedFacts(deck, cardType, facts);
            if (unintroducedFacts.Any())
            {
                var fact = unintroducedFacts.First();
                SetMastery(deck, cardType, fact, cardType.InitialProbability);
                unintroducedFacts = UnintroducedFacts(deck, cardType, facts);
                if (IntroducedFacts(deck, cardType, facts).Count() == 1 & unintroducedFacts.Any())
                {
                    // If this is the first introduced card, we want to introduce another card
                    // (if one exists), simply to get variation in the beginning.
                    fact = unintroducedFacts.First();
                    SetMastery(deck, cardType, fact, cardType.InitialProbability);
                }
            }
        }

        private static void GetWeights(Deck deck, CardType cardType, IEnumerable<Fact> facts, out Dictionary<Fact, double> weights, out double weightSum)
        {
            weights = new Dictionary<Fact, double>();
            weightSum = 0;
            foreach (Fact fact in facts)
            {
                // Math.Pow(1 - mastery, N): the bigger the N,
                // the more the bias towards low-mastery facts.
                var weight = Math.Pow(1 - GetMastery(deck, cardType, fact), 2);
                weights[fact] = weight;
                weightSum += weight;
            }
        }

        public static Fact? GetRandomFact(Random rnd, Deck deck, CardType cardType, Fact? previousFact = null)
        {
            Fact? fact = null;
            var usableFacts = new HashSet<Fact>(UsableFacts(deck, cardType, deck.Facts));
            if (previousFact != null)
            {
                usableFacts.Remove(previousFact);
            }
            if (usableFacts.Any())
            {
                // There exists at least one usable Fact that is not the previous Fact.
                var introducedFacts = IntroducedFacts(deck, cardType, usableFacts);
                if (!BeginnerFacts(deck, cardType, IntroducedFacts(deck, cardType, UsableFacts(deck, cardType, deck.Facts))).Any())
                {
                    // Reconstruct a collection of introduced and usable facts, and see
                    // if there are any facts among them still not at competent level.
                    // If competency has been reached for all facts introduced so far,
                    // it's time to introduce another fact.
                    IntroduceFact(deck, cardType, usableFacts);
                    introducedFacts = IntroducedFacts(deck, cardType, usableFacts);
                }
                var masteredFacts = MasteredFacts(deck, cardType, introducedFacts);
                var unmasteredFacts = UnmasteredFacts(deck, cardType, introducedFacts);
                var numberOfMasteredFacts = masteredFacts.Count();
                var numberOfUnmasteredFacts = unmasteredFacts.Count();
                // When choosing a fact, we mainly want to show still unmastered facts.
                //
                // Consider situation 1: 100 facts have been introduced, 10 are unmastered.
                // In this situation we have enough unmastered facts to choose only among them.
                //
                // Situation 2: 100 introduced facts, 1 unmastered fact. In this situation we want
                // to interleave the practice of this unmastered fact between a number N of reviews
                // of mastered facts. We would probably not want N to be larger than, say, 3 or 4.
                //
                // In situation 1, we want a near 100 % chance of choosing among unmastered facts.
                // In situation 2, we want a maybe 33 % chance of choosing among unmastered facts.
                //
                // Note that Sqrt(x/10) == 100 % for x == 10, and ~32 % for x == 1.
                double threshold = Math.Sqrt(numberOfUnmasteredFacts / 10.0);
                var r = rnd.NextDouble();
                if (r < threshold || numberOfMasteredFacts == 0)
                {
                    // Select among unmastered.
                    var listToUse = unmasteredFacts.OrderBy(f => GetMastery(deck, cardType, f));//rnd.NextDouble());//
                    GetWeights(deck, cardType, listToUse, out Dictionary<Fact, double> weights, out double weightSum);
                    r = rnd.NextDouble() * weightSum;
                    double seenWeights = 0;
                    foreach (var potentialFact in listToUse)
                    {
                        seenWeights += weights[potentialFact];
                        if (seenWeights >= r)
                        {
                            fact = potentialFact;
                            break;
                        }
                    }
                }
                else
                {
                    // Select a mastered card, but not the previous one.
                    var listToUse = masteredFacts.ToList();
                    var i = rnd.Next(listToUse.Count);
                    fact = listToUse[i];
                }
            }
            fact ??= previousFact;
            return fact;
        }

        public static void UpdateFactMastery(Deck deck, CardType cardType, Fact fact, bool wasCorrect)
        {
            var mastery = GetMastery(deck, cardType, fact);
            double conditionalProbability;
            if (wasCorrect)
            {
                double numerator = mastery * (1 - cardType.SlippingProbability);
                double denominator = (mastery * (1 - cardType.SlippingProbability)) + ((1 - mastery) * cardType.LuckyGuessProbability);
                conditionalProbability = numerator / denominator;
            }
            else
            {
                double numerator = mastery * cardType.SlippingProbability;
                double denominator = (mastery * cardType.SlippingProbability) + ((1 - mastery) * (1 - cardType.LuckyGuessProbability));
                conditionalProbability = numerator / denominator;
            }
            mastery = conditionalProbability + (1 - conditionalProbability) * cardType.TransitionProbability;
            SetMastery(deck, cardType, fact, mastery);
        }

        public static string DoTemplateReplacement(Deck deck, Fact fact, string template)
        {
            foreach (var field in deck.Fields)
            {
                var oldString = $"{{{{{field.Name}}}}}";
                var newString = string.Empty;
                if (fact.FieldsContents.ContainsKey(field))
                {
                    newString = fact.FieldsContents[field];
                    if (!field.AllowHTML)
                    {
                        newString = WebUtility.HtmlEncode(newString);
                    }
                }
                template = template.Replace(oldString, newString);
            }
            return template;
        }

        private static bool EqualityCheck<T>(ICollection<T> one, ICollection<T> other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Count != other.Count) { return false; }
            foreach (var oneItem in one)
            {
                if (oneItem is Field)
                {
                    var oneField = oneItem as Field;
                    var otherFields = other as ICollection<Field>;
                    if (!otherFields!.Any(otherField => EqualityCheck(oneField!, otherField))) { return false; }
                }
                else if (oneItem is Fact)
                {
                    var oneFact = oneItem as Fact;
                    var otherFacts = other as ICollection<Fact>;
                    if (!otherFacts!.Any(otherFact => EqualityCheck(oneFact!, otherFact))) { return false; }
                }
                else if (oneItem is CardType)
                {
                    var oneCardType = oneItem as CardType;
                    var otherCardTypes = other as ICollection<CardType>;
                    if (!otherCardTypes!.Any(otherCardType => EqualityCheck(oneCardType!, otherCardType))) { return false; }
                }
                else
                {
                    if (oneItem == null)
                    {
                        var nullSeen = false;
                        foreach (var otherItem in other)
                        {
                            if (otherItem == null)
                            {
                                nullSeen = true;
                                break;
                            }
                        }
                        if (!nullSeen)
                        {
                            return false;
                        }
                    }
                    else if(!other.Any(otherItem => oneItem.Equals(otherItem))) { return false; }
                }
            }
            return true;
        }

        private static bool EqualityCheck(IList one, IList other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Count != other.Count) { return false; }
            for (int i = 0; i < one.Count; i++)
            {
                var oneItem = one[i];
                var otherItem = other[i];
                if (oneItem is Field)
                {
                    var oneField = oneItem as Field;
                    var otherField = otherItem as Field;
                    if (!EqualityCheck(oneField!, otherField!)) { return false; };
                }
                else if (oneItem is Fact)
                {
                    var oneFact = oneItem as Fact;
                    var otherFact = otherItem as Fact;
                    if (!EqualityCheck(oneFact!, otherFact!)) { return false; };
                }
                else if (oneItem is CardType)
                {
                    var oneCardType = oneItem as CardType;
                    var otherCardType = otherItem as CardType;
                    if (!EqualityCheck(oneCardType!, otherCardType!)) { return false; };
                }
                else
                {
                    if (oneItem == null && otherItem == null) { continue; }
                    else if (oneItem == null || otherItem == null) { return false; }
                    else if (!oneItem.Equals(otherItem)) { return false; }
                }
            }
            return true;
        }

        private static bool EqualityCheck<T, U>(IDictionary<T, U> one, IDictionary<T, U> other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Count != other.Count) { return false; }
            if (one.Keys is ICollection<Field>)
            {
                var oneFields = one.Keys as ICollection<Field>;
                var otherFields = other.Keys as ICollection<Field>;
                if (!EqualityCheck(oneFields!, otherFields!)) { return false; }
            }
            else if (one.Keys is ICollection<Fact>)
            {
                var oneFacts = one.Keys as ICollection<Fact>;
                var otherFacts = other.Keys as ICollection<Fact>;
                if (!EqualityCheck(oneFacts!, otherFacts!)) { return false; }
            }
            else if (one.Keys is ICollection<CardType>)
            {
                var oneCardTypes = one.Keys as ICollection<CardType>;
                var otherCardTypes = other.Keys as ICollection<CardType>;
                if (!EqualityCheck(oneCardTypes!, otherCardTypes!)) { return false; }
            }
            else if (!EqualityCheck(one.Keys, other.Keys)) { return false; }

            // Keys are confirmed equal.
            // TODO: Use EqualityCheck(one.Keys, other.Keys) directly?

            foreach (var kvp in one)
            {
                var oneKey = kvp.Key;
                var oneValue = kvp.Value;
                var keyValueMatch = false;

                foreach (var otherKvp in other)
                {
                    var otherKey = otherKvp.Key;
                    var otherValue = otherKvp.Value;

                    var bothKeysAreNull = oneKey == null && otherKey == null;
                    var bothValuesAreNull = oneValue == null && otherValue == null;
                    var neitherKeyIsNull = oneKey != null && otherKey != null;
                    var neitherValueIsNull = oneValue != null && otherValue != null;
                    var keysAreEqual = false;
                    var valuesAreEqual = false;

                    if (neitherKeyIsNull && oneKey is Field)
                    {
                        var oneField = oneKey as Field;
                        var otherField = otherKey as Field;
                        keysAreEqual = EqualityCheck(oneField!, otherField!);
                    }
                    else if (neitherKeyIsNull && oneKey is Fact)
                    {
                        var oneFact = oneKey as Fact;
                        var otherFact = otherKey as Fact;
                        keysAreEqual = EqualityCheck(oneFact!, otherFact!);
                    }
                    else if (neitherKeyIsNull && oneKey is CardType)
                    {
                        var oneCardType = oneKey as CardType;
                        var otherCardType = otherKey as CardType;
                        keysAreEqual = EqualityCheck(oneCardType!, otherCardType!);
                    }
                    else
                    {
                        keysAreEqual = bothKeysAreNull || (neitherKeyIsNull && oneKey!.Equals(otherKey));
                    }

                    if (otherValue is MasteryRecord)
                    {
                        var oneRecord = oneValue as IDictionary<Fact, double>;
                        var otherRecord = otherValue as IDictionary<Fact, double>;
                        valuesAreEqual = EqualityCheck(oneRecord!, otherRecord!);
                    }
                    else if (neitherValueIsNull && otherValue is Array)
                    {
                        var oneArray = oneValue as Array;
                        var otherArray = otherValue as Array;
                        valuesAreEqual = EqualityCheck(oneArray!, otherArray!);
                    }
                    else
                    {
                        valuesAreEqual = bothValuesAreNull || (neitherValueIsNull && oneValue!.Equals(otherValue));
                    }

                    if (keysAreEqual && valuesAreEqual)
                    {
                        keyValueMatch = true;
                        break;
                    }
                }
                if (!keyValueMatch) { return false; }
            }
            return true;
        }

        private static bool EqualityCheck(Field one, Field other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Name != other.Name) { return false; }
            if (one.AllowHTML != other.AllowHTML) { return false; }
            return true;
        }

        private static bool EqualityCheck(Fact one, Fact other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Id != other.Id) { return false; }
            if (!EqualityCheck(one.FieldsContents, other.FieldsContents)) { return false; }
            return true;
        }

        private static bool EqualityCheck(CardType one, CardType other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Id != other.Id) { return false; }
            if (one.Name != other.Name) { return false; }
            if (one.QuestionTemplate != other.QuestionTemplate) { return false; }
            if (one.AnswerTemplate != other.AnswerTemplate) { return false; }
            if (one.Styling != other.Styling) { return false; }
            if (one.MasteryThreshold != other.MasteryThreshold) { return false; }
            if (one.CompetencyThreshold != other.CompetencyThreshold) { return false; }
            if (one.InitialProbability != other.InitialProbability) { return false; }
            if (one.TransitionProbability != other.TransitionProbability) { return false; }
            if (one.SlippingProbability != other.SlippingProbability) { return false; }
            if (one.LuckyGuessProbability != other.LuckyGuessProbability) { return false; }
            if (one.CardsAreComposable != other.CardsAreComposable) { return false; }
            return true;
        }

        public static bool EqualityCheck(Deck one, Deck other)
        {
            if (one == other) { return true; }
            if (one == null || other == null) { return false; }
            if (one.Name != other.Name) { return false; }
            if (one.Description != other.Description) { return false; }
            if (!EqualityCheck(one.Fields, other.Fields)) { return false; }
            if (!EqualityCheck(one.Facts, other.Facts)) { return false; }
            if (!EqualityCheck(one.CardTypes, other.CardTypes)) { return false; }
            if (!EqualityCheck(one.Resources, other.Resources)) { return false; }
            if (!EqualityCheck(one.MasteryRecords, other.MasteryRecords)) { return false; }
            return true;
        }

        public static Deck? FromJson(string json)
        {
            var options = new JsonSerializerOptions()
            {
                Converters = { new DeckConverter() }
            };
            return JsonSerializer.Deserialize<Deck>(json, options);
        }

        public static string ToJson(Deck deck)
        {
            var options = new JsonSerializerOptions()
            {
                Converters = { new DeckConverter() }
            };
            return JsonSerializer.Serialize(deck, options);
        }

        public static bool ExportFile(Deck deck, string path)
        {
            var output = false;

            var json = ToJson(deck);
            if (json != null)
            {
                using StreamWriter writer = new(path);
                writer.Write(json);
            }

            return output;
        }
    }

    public class FieldConverter : JsonConverter<Field>
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
                field = new Field(fieldName, fieldBool??false);
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

    public class MasteryRecordsConverter : JsonConverter<IDictionary<CardType, MasteryRecord>>
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

    public class FactConverter : JsonConverter<Fact>
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

    public class CardTypeConverter : JsonConverter<CardType>
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

    public class DeckConverter : JsonConverter<Deck>
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
                    if (propertyName == nameof(Deck.Name))
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