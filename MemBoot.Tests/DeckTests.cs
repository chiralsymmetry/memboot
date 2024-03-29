﻿using MemBoot.Core.Models;
using MemBoot.DataAccess.Json;

namespace MemBoot.Tests;

public class DeckTests
{
    [Fact]
    public void WorkingThroughDefaultDeckShouldScale()
    {
        // Basically, working through a deck shouldn't cause too much
        // repetition of mastered facts, assuming default probabilities.

        // Arrange
        Deck deck = DeckTestHelpers.CreateAToZDeck();
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
        Deck one = DeckTestHelpers.CreateAToZDeck();
        Deck other = DeckTestHelpers.CreateAToZDeck();
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
        const bool expectedEquality = true;

        // Act
        bool actualEquality = one.IsFunctionallyEqualTo(other);

        // Assert
        Assert.Equal(expectedEquality, actualEquality);
    }

    [Fact]
    public void EqualityCheckForDissimilarDecksShouldGiveFalse()
    {
        // Arrange
        Deck one = DeckTestHelpers.CreateAToZDeck();
        Deck other = DeckTestHelpers.CreateAToZDeck();
        const bool expectedEquality = false;

        // Act
        bool actualEquality = one.IsFunctionallyEqualTo(other);

        // Assert
        Assert.Equal(expectedEquality, actualEquality);
    }

    [Fact]
    public void EqualityCheckForChangedDecksShouldGiveFalse()
    {
        // Arrange
        Deck one = DeckTestHelpers.CreateAToZDeck();
        var cardType = one.CardTypes.First();
        var fact = one.Facts.First();
        string jsonBeforeChange = JsonDeck.ToJson(one);
        Deck? other = JsonDeck.FromJson(jsonBeforeChange);
        const bool expectedEquality = false;

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
        Deck one = DeckTestHelpers.CreateAToZDeck();
        var cardType = one.CardTypes.First();
        var fact = one.Facts.First();
        const bool expectedEquality = true;

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
