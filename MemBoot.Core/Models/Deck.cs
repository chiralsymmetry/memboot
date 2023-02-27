using MemBoot.Core.Extensions;
using System.Net;
using System.Text.RegularExpressions;

namespace MemBoot.Core.Models
{
    public class Deck
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<Fact> Facts { get; set; } = new List<Fact>();
        public ICollection<CardType> CardTypes { get; set; } = new List<CardType>();
        public IDictionary<string, byte[]> Resources { get; set; } = new Dictionary<string, byte[]>();
        public IDictionary<CardType, MasteryRecord> MasteryRecords { get; set; } = new Dictionary<CardType, MasteryRecord>();

        public bool IsFunctionallyEqualTo(Deck other)
        {
            if (this == other) { return true; }
            if (this == null || other == null) { return false; }
            if (Id != other.Id) { return false; }
            if (Name != other.Name) { return false; }
            if (Description != other.Description) { return false; }
            if (!Fields.IsFunctionallyEqualTo(other.Fields)) { return false; }
            if (!Facts.IsFunctionallyEqualTo(other.Facts)) { return false; }
            if (!CardTypes.IsFunctionallyEqualTo(other.CardTypes)) { return false; }
            if (!Resources.IsFunctionallyEqualTo(other.Resources)) { return false; }
            if (!MasteryRecords.IsFunctionallyEqualTo(other.MasteryRecords)) { return false; }
            return true;
        }
        private readonly static Regex TemplateReplacement = new("{{([^}]+)}}", RegexOptions.Compiled);

        public Fact? GetRandomFact(Random rnd, CardType cardType, Fact? previousFact = null)
        {
            Fact? fact = null;
            var usableFacts = new HashSet<Fact>(UsableFacts(cardType, Facts));
            if (previousFact != null)
            {
                usableFacts.Remove(previousFact);
            }
            if (usableFacts.Any())
            {
                // There exists at least one usable Fact that is not the previous Fact.
                var introducedFacts = IntroducedFacts(cardType, usableFacts);
                if (!BeginnerFacts(cardType, IntroducedFacts(cardType, UsableFacts(cardType, Facts))).Any())
                {
                    // Reconstruct a collection of introduced and usable facts, and see
                    // if there are any facts among them still not at competent level.
                    // If competency has been reached for all facts introduced so far,
                    // it's time to introduce another fact.
                    IntroduceFact(cardType, usableFacts);
                    introducedFacts = IntroducedFacts(cardType, usableFacts);
                }
                var masteredFacts = MasteredFacts(cardType, introducedFacts);
                var unmasteredFacts = UnmasteredFacts(cardType, introducedFacts);
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
                    var listToUse = unmasteredFacts.OrderBy(f => GetMastery(cardType, f));//rnd.NextDouble());//
                    GetWeights(cardType, listToUse, out Dictionary<Fact, double> weights, out double weightSum);
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

        public void UpdateFactMastery(CardType cardType, Fact fact, bool wasCorrect)
        {
            var mastery = GetMastery(cardType, fact);
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
            mastery = conditionalProbability + ((1 - conditionalProbability) * cardType.TransitionProbability);
            SetMastery(cardType, fact, mastery);
        }

        public string DoTemplateReplacement(Fact fact, string template)
        {
            foreach (var field in Fields)
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

        /// <summary>
        /// Returns a collection of all <see cref="Field"/>s used in a <see cref="CardType"/>'s templates.
        /// </summary>
        private IEnumerable<Field> FieldsUsedByCardType(CardType cardType)
        {
            // A CardType's question or answer template take the form of strings
            // containing "{{foo}}" where "foo" should be the name of a field.
            // So we add all field objects and remove all those with names
            // not found in the templates.
            ICollection<Field> output = new HashSet<Field>(Fields);
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
        private IEnumerable<Fact> UsableFacts(CardType cardType, IEnumerable<Fact> facts)
        {
            // Add all Facts, then remove all Facts not using all Fields.
            ICollection<Fact> output = new HashSet<Fact>(facts);
            var fieldsUsed = FieldsUsedByCardType(cardType);
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
        private IEnumerable<Fact> IntroducedFacts(CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord record = MasteryRecords[cardType];

                output = facts.Where(f => record.ContainsKey(f));
            }
            return output;
        }

        /// <summary>
        /// Returns a collection of still unintroduced <see cref="Fact"/>s, as a subset of supplied <see cref="Fact"/>s.
        /// A <see cref="CardType"/>-<see cref="Fact"/> combination counts as unintroduced when it has no mastery value in the supplied <see cref="Deck"/>;
        /// we also disregard all <see cref="Fact"/>s not usable with this <see cref="CardType"/>.
        /// </summary>
        private IEnumerable<Fact> UnintroducedFacts(CardType cardType, IEnumerable<Fact> facts)
        {
            // Get all usable Facts, and remove all Facts without mastery values set.
            ICollection<Fact> output = new HashSet<Fact>(UsableFacts(cardType, facts));
            if (MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = MasteryRecords[cardType];
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
        private IEnumerable<Fact> MasteredFacts(CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = MasteryRecords[cardType];
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
        private IEnumerable<Fact> UnmasteredFacts(CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = MasteryRecords[cardType];
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
        private IEnumerable<Fact> BeginnerFacts(CardType cardType, IEnumerable<Fact> facts)
        {
            IEnumerable<Fact> output = Enumerable.Empty<Fact>();
            if (MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecord results = MasteryRecords[cardType];
                output = facts.Where(f => !results.ContainsKey(f) || results[f] < cardType.CompetencyThreshold);
            }
            return output;
        }

        private double GetMastery(CardType cardType, Fact fact)
        {
            double output = cardType.InitialProbability;
            if (MasteryRecords.ContainsKey(cardType) && MasteryRecords[cardType].ContainsKey(fact))
            {
                output = MasteryRecords[cardType][fact];
            }
            return output;
        }

        private void SetMastery(CardType cardType, Fact fact, double mastery)
        {
            if (!MasteryRecords.ContainsKey(cardType))
            {
                MasteryRecords[cardType] = new MasteryRecord();
            }
            MasteryRecords[cardType][fact] = mastery;
        }

        private void IntroduceFact(CardType cardType, IEnumerable<Fact> facts)
        {
            var unintroducedFacts = UnintroducedFacts(cardType, facts);
            if (unintroducedFacts.Any())
            {
                var fact = unintroducedFacts.First();
                SetMastery(cardType, fact, cardType.InitialProbability);
                unintroducedFacts = UnintroducedFacts(cardType, facts);
                if (IntroducedFacts(cardType, facts).Count() == 1 && unintroducedFacts.Any())
                {
                    // If this is the first introduced card, we want to introduce another card
                    // (if one exists), simply to get variation in the beginning.
                    fact = unintroducedFacts.First();
                    SetMastery(cardType, fact, cardType.InitialProbability);
                }
            }
        }

        private void GetWeights(CardType cardType, IEnumerable<Fact> facts, out Dictionary<Fact, double> weights, out double weightSum)
        {
            weights = new Dictionary<Fact, double>();
            weightSum = 0;
            foreach (Fact fact in facts)
            {
                // Math.Pow(1 - mastery, N): the bigger the N,
                // the more the bias towards low-mastery facts.
                var weight = Math.Pow(1 - GetMastery(cardType, fact), 2);
                weights[fact] = weight;
                weightSum += weight;
            }
        }
    }
}
