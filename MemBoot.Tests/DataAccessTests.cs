using MemBoot.Core;
using MemBoot.Core.Models;
using MemBoot.DataAccess;
using MemBoot.DataAccess.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace MemBoot.Tests
{
    public class DataAccessTests
    {
        private static readonly string databaseFileName = "MemBoot.Tests.db";

        private static void DeleteDatabaseBefore()
        {
            if (File.Exists(databaseFileName))
            {
                File.Delete(databaseFileName);
                Assert.True(!File.Exists(databaseFileName));
            }
        }

        private static void DeleteDatabaseAfter()
        {
            File.Delete(databaseFileName);
        }

        [Fact]
        public void RetrievingDeckShouldReturnEquivalentDeck()
        {
            DeleteDatabaseBefore();

            // Arrange
            IDeckStorage deckStorage = new SqliteDeckStorage($"Data Source={databaseFileName};Version=3;");

            Deck createdDeck = DeckTestHelpers.CreateAToZDeck();
            {
                CardType cardType = createdDeck.CardTypes.First();
                Fact fact = createdDeck.Facts.First();
                createdDeck.UpdateFactMastery(cardType, fact, true);
                createdDeck.UpdateFactMastery(cardType, fact, true);
                createdDeck.UpdateFactMastery(cardType, fact, true);
                createdDeck.UpdateFactMastery(cardType, fact, true);
                createdDeck.UpdateFactMastery(cardType, fact, true);
            }
            deckStorage.AddDeck(createdDeck);

            // Act
            Deck? retrievedDeck = deckStorage.GetDeck(createdDeck.Id);

            // Assert
            Assert.NotNull(retrievedDeck);
            Assert.True(createdDeck.IsFunctionallyEqualTo(retrievedDeck));

            DeleteDatabaseAfter();
        }

        [Fact]
        public void ChangesInSqliteDeckShouldBeStored()
        {
            DeleteDatabaseBefore();

            // Arrange
            IDeckStorage deckStorage = new SqliteDeckStorage($"Data Source={databaseFileName};Version=3;");

            Deck createdDeck = DeckTestHelpers.CreateAToZDeck();
            Guid cardTypeId = createdDeck.CardTypes.First().Id;
            deckStorage.AddDeck(createdDeck);

            IFlashcard? flashcard = deckStorage.GetFlashcard(cardTypeId);
            Assert.NotNull(flashcard);

            // Act
            for (int i = 0; i < 5; i++)
            {
                flashcard.Next();
                flashcard.AnswerCorrectly();
            }
            Deck? retrievedDeck = deckStorage.GetDeck(createdDeck.Id);

            // Assert
            Assert.NotNull(retrievedDeck);
            Assert.False(createdDeck.IsFunctionallyEqualTo(retrievedDeck));

            DeleteDatabaseAfter();
        }
    }
}
