using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemBoot;
using Xunit.Abstractions;

namespace MemBoot.Tests
{
    public class FlashcardTests
    {
        [Theory]
        [InlineData(5,
            new bool[] {  },
            new bool[] {  })]
        [InlineData(5,
            new bool[] { false },
            new bool[] { false })]
        [InlineData(5,
            new bool[] { false, true, false },
            new bool[] { false, true, false })]
        [InlineData(5,
            new bool[] { false, true, false, true, true },
            new bool[] { false, true, false, true, true })]
        [InlineData(5,
            new bool[] { false, true, false, true, true, false, true, false, false },
            new bool[] { true, false, true, false, false })]
        public void AnswerHistoryWorks(int size, bool[] answersToGive, bool[] expectedAnswers)
        {
            // Arrange
            Flashcard flashcard = new Flashcard(size);

            // Act
            foreach (var answer in answersToGive)
            {
                flashcard.Answer(answer);
            }
            var actualAnswers = flashcard.Answers;

            // Assert
            Assert.True(actualAnswers.SequenceEqual(expectedAnswers));
        }

        [Theory]
        [InlineData(17,
            new bool[] { },
            0)]
        [InlineData(17,
            new bool[] { false },
            0)]
        [InlineData(17,
            new bool[] { true },
            1)]
        [InlineData(17,
            new bool[] { false, true, false },
            1.0 / 3.0)]
        [InlineData(17,
            new bool[] { false, true, false, true, true, false, true, false, false },
            4.0 / 9.0)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            10.0 / 17.0)]
        public void AnswerAccuracyWorks(int size, bool[] answersToGive, float expectedAccuracy)
        {
            // Arrange
            Flashcard flashcard = new Flashcard(size);
            float delta = 0.01f;
            float expectedMin = Math.Max(expectedAccuracy - delta, 0);
            float expectedMax = Math.Min(expectedAccuracy + delta, 1);

            // Act
            foreach (var answer in answersToGive)
            {
                flashcard.Answer(answer);
            }
            float actualAccuracy = flashcard.Accuracy;

            // Assert
            Assert.InRange(actualAccuracy, expectedMin, expectedMax);
        }
    }
}
