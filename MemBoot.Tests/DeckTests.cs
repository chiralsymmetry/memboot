using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MemBoot.Tests
{
    public class DeckTests
    {
        private static byte[] Image => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAABhGlDQ1BJQ0MgcHJvZmlsZQAAKJF9kT1Iw0AcxV9TRdGKgh1EHCJUJwuioo5SxSJYKG2FVh1MLv2CJg1Jiouj4Fpw8GOx6uDirKuDqyAIfoC4ujgpukiJ/0sKLWI8OO7Hu3uPu3eAUCsx1WwbB1TNMhLRiJjOrIodrxDQh27MYFhiph5LLqbgOb7u4ePrXZhneZ/7c/QoWZMBPpF4jumGRbxBPL1p6Zz3iYOsICnE58RjBl2Q+JHrsstvnPMOCzwzaKQS88RBYjHfwnILs4KhEk8RhxRVo3wh7bLCeYuzWqqwxj35CwNZbSXJdZpDiGIJMcQhQkYFRZRgIUyrRoqJBO1HPPyDjj9OLplcRTByLKAMFZLjB/+D392auckJNykQAdpfbPtjBOjYBepV2/4+tu36CeB/Bq60pr9cA2Y/Sa82tdAR0LsNXFw3NXkPuNwBBp50yZAcyU9TyOWA9zP6pgzQfwt0rbm9NfZx+gCkqKvlG+DgEBjNU/a6x7s7W3v790yjvx+18HLC8Oz/8gAAAAlwSFlzAAAuIwAALiMBeKU/dgAAAAd0SU1FB+cCEAs6EjcJxbUAAAAZdEVYdENvbW1lbnQAQ3JlYXRlZCB3aXRoIEdJTVBXgQ4XAAAAoklEQVQY032QsQmDUABEn78RJEP8JhOIE2gbrP8qFrqBa8Q+rmEt6AJiI4E0CnopJCEE8erj8e48ARi4bbgXV+BCb7jDAzaEEVnPmAdKQiWhikA9o8iEQaQ9YxyJemboGDrqJY72RspKVQSinv2tdLJO1t9K6jkPtFLxpElCMXROdmqZWpwsQ5eEetIYzvOBL3/wYof/qC0Hat9hxdEw7/yWN0fzetlZBLpkAAAAAElFTkSuQmCC");

        private static Deck CreateAToZDeck()
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
                deck.Resources.Add("img.png", Image);
            }

            return deck;
        }

        [Fact]
        public void WorkingThroughDefaultDeckShouldScale()
        {
            // Basically, working through a deck shouldn't cause too much
            // repetition of mastered facts, assuming default probabilities.

            // Arrange
            Deck deck = CreateAToZDeck();
            int numberOfFacts = deck.Facts.Count;
            var cardType = deck.CardTypes.First();
            var frontField = deck.Fields.First(f => f.Name == "front");

            Dictionary<Fact, int> hitCounts = new();
            Fact? fact;
            int times = 1000 * numberOfFacts;
            int timesUntilAllMastered = 0;
            Random rnd = new();

            // Act
            for (int i = 0; i < times; i++)
            {
                fact = DeckProcessor.GetRandomFact(rnd, deck, cardType);
                if (fact != null)
                {
                    if (timesUntilAllMastered == 0 && fact[frontField] == "_")
                    {
                        timesUntilAllMastered = i;
                    }
                    DeckProcessor.UpdateFactMastery(deck, cardType, fact, true);
                    if (!hitCounts.ContainsKey(fact))
                    {
                        hitCounts[fact] = 0;
                    }
                    hitCounts[fact]++;
                }
            }

            // Assert

            // Each fact should be mastered by about 5 tries. But they shouldn't be shown
            // repeatedly, so mastered facts should be shown in between. This means that
            // earlier facts will have been shown the most, and newer facts the least.
            // The average fact should be shown something like 18 times before all
            // facts have been mastered.
            double actualAverageHits = timesUntilAllMastered / (numberOfFacts - 1);
            Assert.InRange(actualAverageHits, 14.0, 21.0);
            // After working far beyond mastery, all facts should have been shown with
            // near-same frequency.
            actualAverageHits = times / numberOfFacts;
            double low = actualAverageHits * 0.85;
            double high = actualAverageHits * 1.15;
            foreach (var hitCount in hitCounts)
            {
                Assert.InRange(hitCount.Value, low, high);
            }
        }

        [Fact]
        public void EqualityCheckForClonedDecksShouldGiveTrue()
        {
            // Arrange
            Deck one = CreateAToZDeck();
            Deck other = CreateAToZDeck();
            var oneField = one.Fields.First(f => f.Name == "front");
            var otherField = other.Fields.First(f => f.Name == "front");
            foreach (var oneFact in one.Facts)
            {
                foreach (var otherFact in other.Facts)
                {
                    if (oneFact.FieldsContents[oneField] == otherFact.FieldsContents[otherField])
                    {
                        otherFact.Id = oneFact.Id;
                    }
                }
            }
            foreach (var oneCardType in one.CardTypes)
            {
                foreach (var otherCardType in other.CardTypes)
                {
                    if (oneCardType.Name == otherCardType.Name)
                    {
                        otherCardType.Id = oneCardType.Id;
                    }
                }
            }
            bool expectedEquality = true;

            // Act
            bool actualEquality = DeckProcessor.EqualityCheck(one, other);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void EqualityCheckForDissimilarDecksShouldGiveFalse()
        {
            // Arrange
            Deck one = CreateAToZDeck();
            Deck other = CreateAToZDeck();
            bool expectedEquality = false;

            // Act
            bool actualEquality = DeckProcessor.EqualityCheck(one, other);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void JsonSerializingAndDeserializingShouldGiveEqualDecks()
        {
            // Arrange
            Deck one = CreateAToZDeck();
            bool expectedEquality = true;

            // Act
            string json = DeckProcessor.ToJson(one);
            Deck? other = DeckProcessor.FromJson(json);
            bool actualEquality = DeckProcessor.EqualityCheck(one, other!);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }
    }
}
