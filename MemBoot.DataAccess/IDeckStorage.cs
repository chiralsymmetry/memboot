using MemBoot.Core;
using MemBoot.Core.Models;

namespace MemBoot.DataAccess
{
    public interface IDeckStorage
    {
        bool AddDeck(Deck deck);
        Deck? GetDeck(Guid deckId);
        ICollection<Tuple<string, Guid>> GetCardTypeIds();
        IFlashcard? GetFlashcard(Guid cardTypeId);
    }
}