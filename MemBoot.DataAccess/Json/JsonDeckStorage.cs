using MemBoot.Core;
using MemBoot.Core.Models;

namespace MemBoot.DataAccess.Json
{
    public class JsonDeckStorage : IDeckStorage
    {

        private readonly ICollection<Deck> decks;
        private readonly string saveFilePath;

        public JsonDeckStorage(string saveFilePath)
        {
            decks = new HashSet<Deck>();
            this.saveFilePath = saveFilePath;
        }

        public ICollection<Tuple<string, Guid>> GetCardTypeIds()
        {
            var output = new List<Tuple<string, Guid>>();

            foreach (var deck in decks)
            {
                foreach (var cardType in deck.CardTypes)
                {
                    output.Add(new(cardType.Name, cardType.Id));
                }
            }

            return output;
        }

        public Deck? GetDeck(Guid deckId)
        {
            return decks.FirstOrDefault(d => d.Id.Equals(deckId));
        }

        public bool AddDeck(Deck deck)
        {
            decks.Add(deck);
            return true;
        }

        public IFlashcard? GetFlashcard(Guid cardTypeId)
        {
            IFlashcard? output = null;

            foreach (var deck in decks)
            {
                CardType? cardType = deck.CardTypes.FirstOrDefault(ct => ct.Id.Equals(cardTypeId));
                if (cardType != null)
                {
                    output = new JsonDeck(deck, cardType, saveFilePath);
                }
            }

            return output;
        }
    }
}
