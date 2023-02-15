using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class Deck
    {
        private readonly IList<Flashcard> cards = new List<Flashcard>();
        private readonly IDictionary<Flashcard, bool> cardHasBeenIntroduced = new Dictionary<Flashcard, bool>();
        private Flashcard? previouslyChosenCard = null;

        private readonly Random rnd = new();
        private readonly double MasteryThreshold;
        private readonly double CompetencyThreshold;
        private readonly double InitialProbability;
        private readonly double TransitionProbability;
        private readonly double SlippingProbability;
        private readonly double LuckyGuessProbability;

        public bool CardsAreComposable { get; }

        public Deck(double init, double transit, double slip, double guess, double masteryThreshold, double competencyThreshold, IEnumerable<Flashcard> cards, bool cardsAreComposable)
        {
            InitialProbability = init;
            TransitionProbability = transit;
            SlippingProbability = slip;
            LuckyGuessProbability = guess;
            MasteryThreshold = masteryThreshold;
            CompetencyThreshold = competencyThreshold;
            CardsAreComposable = cardsAreComposable;
            foreach (var card in cards)
            {
                this.cards.Add(card);
                cardHasBeenIntroduced[card] = card.Mastery >= MasteryThreshold;
            }
        }

        public Deck() : this(0.0, 0.1, 0.1, 1.0/3.0, 0.95, 0.85, Array.Empty<Flashcard>(), false) { }

        public void AddFlashcard(Flashcard card)
        {
            cards.Add(card);
            cardHasBeenIntroduced[card] = card.Mastery >= MasteryThreshold;
        }

        public bool RemoveFlashcard(Flashcard card)
        {
            bool result = true;
            result &= cards.Remove(card);
            result &= cardHasBeenIntroduced.Remove(card);
            return result;
        }

        public void CreateFlashcard(string question, string answer)
        {
            Flashcard card = new(question, answer, InitialProbability);
            cards.Add(card);
            cardHasBeenIntroduced[card] = card.Mastery >= MasteryThreshold;
        }

        public void ResetFlashcard(Flashcard card)
        {
            card.Mastery = InitialProbability;
            if (cards.Contains(card))
            {
                cardHasBeenIntroduced[card] = card.Mastery >= MasteryThreshold;
            }
        }

        private void UpdateFlashcardMastery(Flashcard card, bool wasCorrect)
        {
            double conditionalProbability;
            if (wasCorrect)
            {
                double numerator = card.Mastery * (1 - SlippingProbability);
                double denominator = (card.Mastery * (1 - SlippingProbability)) + ((1 - card.Mastery) * LuckyGuessProbability);
                conditionalProbability = numerator / denominator;
            }
            else
            {
                double numerator = card.Mastery * SlippingProbability;
                double denominator = (card.Mastery * SlippingProbability) + ((1 - card.Mastery) * (1 - LuckyGuessProbability));
                conditionalProbability = numerator / denominator;
            }
            double updatedMastery = conditionalProbability + (1 - conditionalProbability) * TransitionProbability;
            card.Mastery = updatedMastery;
        }

        public void AnswerCorrectly(Flashcard card)
        {
            UpdateFlashcardMastery(card, true);
        }

        public void AnswerIncorrectly(Flashcard card)
        {
            UpdateFlashcardMastery(card, false);
        }

        private IEnumerable<Flashcard> IntroducedSubset(IEnumerable<Flashcard> flashcards)
        {
            return flashcards.Where(card => cardHasBeenIntroduced[card] == true);
        }

        private IEnumerable<Flashcard> UnintroducedSubset(IEnumerable<Flashcard> flashcards)
        {
            return flashcards.Where(card => cardHasBeenIntroduced[card] == false);
        }

        private IEnumerable<Flashcard> MasteredSubset(IEnumerable<Flashcard> flashcards)
        {
            return flashcards.Where(card => card.Mastery >= MasteryThreshold);
        }

        private IEnumerable<Flashcard> UnmasteredSubset(IEnumerable<Flashcard> flashcards)
        {
            return flashcards.Where(card => card.Mastery < MasteryThreshold);
        }

        private IEnumerable<Flashcard> BeginnerSubset(IEnumerable<Flashcard> flashcards)
        {
            return flashcards.Where(card => card.Mastery < CompetencyThreshold);
        }

        private void IntroduceFlashcard()
        {
            var unintroducedCards = UnintroducedSubset(cards);
            if (unintroducedCards.Any())
            {
                var card = unintroducedCards.First();
                cardHasBeenIntroduced[card] = true;
                unintroducedCards = UnintroducedSubset(cards);
                if (IntroducedSubset(cards).Count() == 1 && unintroducedCards.Any())
                {
                    // If this is the first introduced card, we want to introduce another card
                    // (if one exists), simply to get variation in the beginning.
                    card = unintroducedCards.First();
                    cardHasBeenIntroduced[card] = true;
                }
            }
        }

        private static void GetWeights(IEnumerable<Flashcard> flashcards, out Dictionary<Flashcard, double> weights, out double weightSum)
        {
            weights = new Dictionary<Flashcard, double>();
            weightSum = 0;
            foreach (var card in flashcards)
            {
                var weight = Math.Pow(1 - card.Mastery, 1);
                weights[card] = weight;
                weightSum += weight;
            }
        }

        public Flashcard? RandomIntroducedCard()
        {
            Flashcard? card = null;
            if (cards.Count > 0)
            {
                card = cards.First();
                var introducedCards = IntroducedSubset(cards);
                if (!BeginnerSubset(introducedCards).Any())
                {
                    // If competency has been reached for all cards introduced so far, it's time to introduce another card.
                    IntroduceFlashcard();
                }
                var masteredCards = MasteredSubset(introducedCards);
                var unmasteredCards = UnmasteredSubset(introducedCards);
                var numberOfMasteredCards = masteredCards.Count();
                var numberOfUnmasteredCards = unmasteredCards.Count();
                // When choosing a card, we mainly want to show still unmastered cards.
                //
                // Consider situation 1: 100 cards have been introduced, 10 are unmastered.
                // In this situation we have enough unmastered cards to choose only among them.
                //
                // Situation 2: 100 introduced cards, 1 unmastered card. In this situation we want
                // to interleave the practice of this unmastered card between a number N of reviews
                // of mastered cards. We would probably not want N to be larger than, say, 3 or 4.
                //
                // In situation 1, we want a near 100 % chance of choosing among unmastered cards.
                // In situation 2, we want a maybe 33 % chance of choosing among unmastered cards.
                double threshold = Math.Sqrt(numberOfUnmasteredCards / 10.0);
                var r = rnd.NextDouble();
                if (r < threshold || numberOfMasteredCards == 0)
                {
                    // Select among unmastered.
                    var listToUse = unmasteredCards.OrderBy(c => c.Mastery);
                    GetWeights(listToUse, out Dictionary<Flashcard, double> weights, out double weightSum);
                    r = rnd.NextDouble() * weightSum;
                    double seenWeights = 0;
                    foreach (var potentialCard in listToUse)
                    {
                        seenWeights += weights[potentialCard];
                        if (seenWeights >= r)
                        {
                            card = potentialCard;
                            break;
                        }
                    }
                }
                else
                {
                    var listToUse = masteredCards.ToList();
                    var i = rnd.Next(listToUse.Count);
                    card = listToUse[i];
                }
            }
            if (previouslyChosenCard == card)
            {
                card = RandomIntroducedCard();
            }
            previouslyChosenCard = card;
            return card;
        }
    }
}
