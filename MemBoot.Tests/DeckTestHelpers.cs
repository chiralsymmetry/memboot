using MemBoot.Core.Models;

namespace MemBoot.Tests
{
    internal static class DeckTestHelpers
    {
        internal static byte[] Image => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAABhGlDQ1BJQ0MgcHJvZmlsZQAAKJF9kT1Iw0AcxV9TRdGKgh1EHCJUJwuioo5SxSJYKG2FVh1MLv2CJg1Jiouj4Fpw8GOx6uDirKuDqyAIfoC4ujgpukiJ/0sKLWI8OO7Hu3uPu3eAUCsx1WwbB1TNMhLRiJjOrIodrxDQh27MYFhiph5LLqbgOb7u4ePrXZhneZ/7c/QoWZMBPpF4jumGRbxBPL1p6Zz3iYOsICnE58RjBl2Q+JHrsstvnPMOCzwzaKQS88RBYjHfwnILs4KhEk8RhxRVo3wh7bLCeYuzWqqwxj35CwNZbSXJdZpDiGIJMcQhQkYFRZRgIUyrRoqJBO1HPPyDjj9OLplcRTByLKAMFZLjB/+D392auckJNykQAdpfbPtjBOjYBepV2/4+tu36CeB/Bq60pr9cA2Y/Sa82tdAR0LsNXFw3NXkPuNwBBp50yZAcyU9TyOWA9zP6pgzQfwt0rbm9NfZx+gCkqKvlG+DgEBjNU/a6x7s7W3v790yjvx+18HLC8Oz/8gAAAAlwSFlzAAAuIwAALiMBeKU/dgAAAAd0SU1FB+cCEAs6EjcJxbUAAAAZdEVYdENvbW1lbnQAQ3JlYXRlZCB3aXRoIEdJTVBXgQ4XAAAAoklEQVQY032QsQmDUABEn78RJEP8JhOIE2gbrP8qFrqBa8Q+rmEt6AJiI4E0CnopJCEE8erj8e48ARi4bbgXV+BCb7jDAzaEEVnPmAdKQiWhikA9o8iEQaQ9YxyJemboGDrqJY72RspKVQSinv2tdLJO1t9K6jkPtFLxpElCMXROdmqZWpwsQ5eEetIYzvOBL3/wYof/qC0Hat9hxdEw7/yWN0fzetlZBLpkAAAAAElFTkSuQmCC");

        internal static Deck CreateAToZDeck()
        {
            var frontSides = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "_" };
            var backSides = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "_" };

            var deck = new Deck();

            {
                // Create fields.
                var fieldNames = new string[] { "front", "back" };
                foreach (var fieldName in fieldNames)
                {
                    var field = new Field(fieldName);
                    deck.Fields.Add(field);
                }
            }

            {
                // Create facts.
                Field frontField = deck.Fields.First(f => f.Name == "front");
                Field backField = deck.Fields.First(f => f.Name == "back");
                for (int i = 0; i < frontSides.Length; i++)
                {
                    var frontSide = frontSides[i];
                    var backSide = backSides[i];
                    Guid guid = Guid.NewGuid();
                    var newFact = new Fact(guid, deck.Fields);
                    newFact[frontField] = frontSide;
                    newFact[backField] = backSide;
                    deck.Facts.Add(newFact);
                }
            }

            {
                // Create a card type.
                var cardType = new CardType(Guid.NewGuid(), "Front-to-back", "{{front}}", "{{back}}");
                deck.CardTypes.Add(cardType);
            }

            {
                // Create a resource.
                //deck.Resources.Add("img.png", Image);
            }

            return deck;
        }
    }
}