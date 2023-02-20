using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class DeckStorage
    {
        private readonly ICollection<Deck> decks;
        public DeckStorage()
        {
            decks = new HashSet<Deck>();
        }

        public bool ImportFile(string path)
        {
            var output = false;

            if (System.IO.File.Exists(path))
            {
                var json = System.IO.File.ReadAllText(path);
                var deck = DeckProcessor.FromJson(json);
                if (deck != null)
                {
                    decks.Add(deck);
                    output = true;
                }
            }

            return output;
        }

        public ICollection<Tuple<Deck, CardType>> GetCardTypes()
        {
            var output = new HashSet<Tuple<Deck, CardType>>();

            foreach (var deck in decks)
            {
                foreach (var cardType in deck.CardTypes)
                {
                    output.Add(new(deck, cardType));
                }
            }

            return output;
        }

        public (Deck?, CardType?) GetCardType(Guid id)
        {
            (Deck?, CardType?) output = (null, null);

            foreach (var deck in decks)
            {
                foreach (var cardType in deck.CardTypes)
                {
                    if (cardType.Id == id)
                    {
                        output.Item1 = deck;
                        output.Item2 = cardType;
                        break;
                    }
                }
            }

            return output;
        }
    }
}
