using MemBoot.Core;
using MemBoot.Core.Models;

namespace MemBoot.DataAccess;

public interface IDeckStorage
{
    IEnumerable<Deck> GetDecks();
    bool AddDeck(Deck deck);
    bool AddOrReplaceDeck(Deck currentDeck);
    bool RemoveDeck(Deck deck);
    Deck? GetDeck(Guid deckId);
    ICollection<Tuple<string, Guid>> GetCardTypeIds();
    IFlashcard? GetFlashcard(Guid cardTypeId);
    Deck? GetDeckFromCardTypeId(Guid cardTypeId);
}