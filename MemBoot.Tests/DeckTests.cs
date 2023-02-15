using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot.Tests
{
    public class DeckTests
    {
        [Fact]
        public void WorkingThroughDefaultDeckShouldScale()
        {
            // Basically, working through a deck shouldn't cause too much repetition of mastered cards.

            // Arrange
            var deck = new Deck();
            deck.CreateFlashcard("a", "a");
            deck.CreateFlashcard("b", "b");
            deck.CreateFlashcard("c", "c");
            deck.CreateFlashcard("d", "d");
            deck.CreateFlashcard("e", "e");
            deck.CreateFlashcard("f", "f");
            deck.CreateFlashcard("g", "g");
            deck.CreateFlashcard("h", "h");
            deck.CreateFlashcard("i", "i");
            deck.CreateFlashcard("j", "j");
            deck.CreateFlashcard("k", "k");
            deck.CreateFlashcard("l", "l");
            deck.CreateFlashcard("m", "m");
            deck.CreateFlashcard("n", "n");
            deck.CreateFlashcard("o", "o");
            deck.CreateFlashcard("p", "p");
            deck.CreateFlashcard("q", "q");
            deck.CreateFlashcard("r", "r");
            deck.CreateFlashcard("s", "s");
            deck.CreateFlashcard("t", "t");
            deck.CreateFlashcard("u", "u");
            deck.CreateFlashcard("v", "v");
            deck.CreateFlashcard("w", "w");
            deck.CreateFlashcard("x", "x");
            deck.CreateFlashcard("y", "y");
            deck.CreateFlashcard("z", "z");
            deck.CreateFlashcard("_", "_");
            Dictionary<Flashcard, int> hitCounts = new();
            Flashcard? card;
            int times = 10_000;
            int timesUntilAllMastered = 0;

            // Act
            for (int i = 0; i < times; i++)
            {
                card = deck.RandomIntroducedCard();
                if (card != null)
                {
                    if (timesUntilAllMastered == 0 && card.QuestionSide == "_")
                    {
                        timesUntilAllMastered = i;
                    }
                    deck.AnswerCorrectly(card);
                    if (!hitCounts.ContainsKey(card))
                    {
                        hitCounts[card] = 0;
                    }
                    hitCounts[card]++;
                }
            }

            // Assert

            // Each card should be mastered by about 5 tries. But they shouldn't be shown
            // repeatedly, so mastered cards should be shown in between. This means that
            // earlier cards will have been shown the most, and newer cards the least.
            // The average card should be shown something like 12 to 18 times before all
            // cards have been mastered.
            double expectedAverageHits = timesUntilAllMastered / 26.0;
            Assert.InRange(expectedAverageHits, 12.0, 18.0);
            // After working far beyond mastery, all cards should have been shown with
            // near-same frequency.
            expectedAverageHits = times / 27.0;
            double low = expectedAverageHits * 0.80;
            double high = expectedAverageHits * 1.20;
            foreach (var hitCount in hitCounts)
            {
                Assert.InRange(hitCount.Value, low, high);
            }
        }
    }
}
