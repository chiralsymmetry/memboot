using MemBoot.Core.Extensions;
using MemBoot.Core.Models;
using MemBoot.DataAccess.Json;
using Microsoft.VisualBasic;
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

            Dictionary<Fact, int> firstRunHitCounts = new();
            Dictionary<Fact, int> hitCounts = new();
            Fact? fact;
            int times = 1000 * numberOfFacts;
            Random rnd = new();

            // Act
            do
            {
                fact = deck.GetRandomFact(rnd, cardType);
                if (fact != null)
                {
                    deck.UpdateFactMastery(cardType, fact, true);
                    if (!firstRunHitCounts.ContainsKey(fact))
                    {
                        firstRunHitCounts[fact] = 0;
                    }
                    firstRunHitCounts[fact]++;
                }
            } while (fact != null && fact[frontField] != "_");

            for (int i = 0; i < times; i++)
            {
                fact = deck.GetRandomFact(rnd, cardType);
                if (fact != null)
                {
                    deck.UpdateFactMastery(cardType, fact, true);
                    if (!hitCounts.ContainsKey(fact))
                    {
                        hitCounts[fact] = 0;
                    }
                    hitCounts[fact]++;
                }
            }

            // Assert
            var firstRunSum = firstRunHitCounts.Values.Sum();
            var firstRunAverage = firstRunHitCounts.Values.Average();
            var firstRunVariance = firstRunHitCounts.Values.Average(num => Math.Pow(num - firstRunAverage, 2));
            var firstRunStdDev = Math.Sqrt(firstRunVariance);
            // With default settings, a Fact should reach a competent level after 5 correct answers,
            // and mastery after 6 correct answers (if no incorrect answers). Thus the sum of all
            // answers until all N Facts are at a competent level is at the absolute minimum 5N.
            // Since the algorithm interleaves cards, the average is actually about 7.75N, but
            // due to randomness it can go as low as slightly below 6N, and as high as slightly
            // below 11N. Variance can be high, but from experiments, we may expect standard
            // deviation to never go as high as 9.
            Assert.InRange(firstRunAverage, 5.5, 11.5);
            Assert.InRange(firstRunStdDev, 0.25, 9.0);

            var totalAverage = hitCounts.Values.Average();
            var totalVariance = hitCounts.Values.Average(num => Math.Pow(num - totalAverage, 2));
            var totalStdDev = Math.Sqrt(totalVariance);
            // When answering 1000N questions after all Facts reached a competent level,
            // average answers per Fact is 1000 (obviously), and experiments show standard
            // deviation above 15 and below 45.
            Assert.InRange(totalStdDev, 15.0, 45.0);
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
            bool actualEquality = one.IsFunctionallyEqualTo(other);

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
            bool actualEquality = one.IsFunctionallyEqualTo(other);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void EqualityCheckForChangedDecksShouldGiveFalse()
        {
            // Arrange
            Deck one = CreateAToZDeck();
            var cardType = one.CardTypes.First();
            var fact = one.Facts.First();
            string jsonBeforeChange = JsonDeck.ToJson(one);
            Deck? other = JsonDeck.FromJson(jsonBeforeChange);
            bool expectedEquality = false;

            // Act
            one.UpdateFactMastery(cardType, fact, true);
            one.UpdateFactMastery(cardType, fact, true);
            one.UpdateFactMastery(cardType, fact, true);
            bool actualEquality = one.IsFunctionallyEqualTo(other!);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void JsonSerializingAndDeserializingShouldGiveEqualDecks()
        {
            // Arrange
            Deck one = CreateAToZDeck();
            var cardType = one.CardTypes.First();
            var fact = one.Facts.First();
            bool expectedEquality = true;

            // Act
            one.UpdateFactMastery(cardType, fact, true);
            one.UpdateFactMastery(cardType, fact, true);
            one.UpdateFactMastery(cardType, fact, true);
            string json = JsonDeck.ToJson(one);
            Deck? other = JsonDeck.FromJson(json);
            bool actualEquality = one.IsFunctionallyEqualTo(other!);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }
    }
}
