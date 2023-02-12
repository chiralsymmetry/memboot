using System;
using System.Collections.Generic;
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

        private readonly Random rnd = new();
        private readonly float MasteryThreshold;
        private readonly float InitialProbability;
        private readonly float TransitionProbability;
        private readonly float SlippingProbability;
        private readonly float LuckyGuessProbability;

        public bool CardsAreComposable { get; }

        public Deck(float init, float transit, float slip, float guess, float masteryThreshold, IEnumerable<Flashcard> cards, bool cardsAreComposable)
        {
            InitialProbability = init;
            TransitionProbability = transit;
            SlippingProbability = slip;
            LuckyGuessProbability = guess;
            MasteryThreshold = masteryThreshold;
            CardsAreComposable = cardsAreComposable;
            foreach (var card in cards)
            {
                this.cards.Add(card);
                cardHasBeenIntroduced[card] = card.Mastery >= MasteryThreshold;
            }
        }

        public Deck() : this(0.0f, 0.1f, 0.1f, 0.33f, 0.95f, Array.Empty<Flashcard>(), false) { }

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
                conditionalProbability = (float)(numerator / denominator);
            }
            else
            {
                double numerator = card.Mastery * SlippingProbability;
                double denominator = (card.Mastery * SlippingProbability) + ((1 - card.Mastery) * (1 - LuckyGuessProbability));
                conditionalProbability = (float)(numerator / denominator);
            }
            double updatedMastery = conditionalProbability + (1 - conditionalProbability) * TransitionProbability;
            card.Mastery = (float)updatedMastery;
        }

        public void AnswerCorrectly(Flashcard card)
        {
            UpdateFlashcardMastery(card, true);
        }

        public void AnswerIncorrectly(Flashcard card)
        {
            UpdateFlashcardMastery(card, false);
        }

        private bool AllIntroducedFlashcardsAreMastered()
        {
            var unmastered = IntroducedFlashcards().Where(card => card.Mastery < MasteryThreshold).Count();
            return unmastered == 0;
        }

        private IList<Flashcard> IntroducedFlashcards()
        {
            return cardHasBeenIntroduced.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        private IList<Flashcard> UnintroducedFlashcards()
        {
            return cardHasBeenIntroduced.Where(kvp => kvp.Value == false).Select(kvp => kvp.Key).ToList();
        }

        private IList<Flashcard> MasteredFlashcards()
        {
            return cards.Where(card => card.Mastery >= MasteryThreshold).ToList();
        }

        private IList<Flashcard> UnmasteredFlashcards()
        {
            return cards.Where(card => card.Mastery < MasteryThreshold).ToList();
        }

        private void IntroduceFlashcard()
        {
            var card = UnintroducedFlashcards().First();
            cardHasBeenIntroduced[card] = true;
        }

        public Flashcard? RandomIntroducedCard()
        {
            Flashcard? card = null;
            if (cards.Count > 0)
            {
                if (AllIntroducedFlashcardsAreMastered())
                {
                    IntroduceFlashcard();
                }
                if (IntroducedFlashcards().Count == 1 && UnintroducedFlashcards().Count > 0)
                {
                    IntroduceFlashcard();
                }
                var introduced = IntroducedFlashcards();
                while (card == null)
                {
                    var i = rnd.Next(introduced.Count);
                    var potentialCard = introduced[i];
                    var r = (float)rnd.NextDouble();
                    if (potentialCard.Mastery < r)
                    {
                        card = potentialCard;
                    }
                }
            }
            return card;
        }
    }
}
