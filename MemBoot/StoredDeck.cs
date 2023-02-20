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
        private readonly string savePath;
        private Fact? currentFact = null;

        public StoredDeck(Deck deck, CardType cardType, string savePath)
        {
            this.deck = deck;
            this.cardType = cardType;
            this.savePath = savePath;
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
                SaveDeck();
            }
        }

        public void AnswerIncorrectly()
        {
            if (currentFact != null)
            {
                DeckProcessor.UpdateFactMastery(deck, cardType, currentFact, false);
                SaveDeck();
            }
        }

        public void Next()
        {
            currentFact = DeckProcessor.GetRandomFact(rnd, deck, cardType, currentFact);
        }

        private void SaveDeck()
        {
            DeckProcessor.ExportFile(deck, savePath);
        }
    }
}
