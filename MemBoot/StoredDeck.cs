using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class StoredDeck : IDeck
    {
        private readonly Random rnd = new();
        private readonly Deck deck;
        private readonly CardType cardType;
        private Fact? currentFact = null;

        public StoredDeck(Deck deck, CardType cardType)
        {
            this.deck = deck;
            this.cardType = cardType;
        }

        string IDeck.CurrentQuestion
        {
            get
            {
                string output = string.Empty;
                if (currentFact != null)
                {
                    output = DeckProcessor.DoTemplateReplacement(deck, currentFact, cardType.QuestionTemplate);
                }
                return output;
            }
        }

        string IDeck.CurrentAnswer
        {
            get
            {
                string output = string.Empty;
                if (currentFact != null)
                {
                    output = DeckProcessor.DoTemplateReplacement(deck, currentFact, cardType.AnswerTemplate);
                }
                return output;
            }
        }

        public void AnswerCorrectly()
        {
            if (currentFact != null)
            {
                DeckProcessor.UpdateFactMastery(deck, cardType, currentFact, true);
            }
        }

        public void AnswerIncorrectly()
        {
            if (currentFact != null)
            {
                DeckProcessor.UpdateFactMastery(deck, cardType, currentFact, false);
            }
        }

        public void Next()
        {
            currentFact = DeckProcessor.GetRandomFact(rnd, deck, cardType, currentFact);
        }
    }
}
