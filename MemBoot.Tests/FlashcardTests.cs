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
        public void AnswerHistoryShouldWork(int size, bool[] answersToGive, bool[] expectedAnswers)
        {
            // Arrange
            Flashcard flashcard = new(size);

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
        public void AnswerAccuracyShouldWork(int size, bool[] answersToGive, float expectedAccuracy)
        {
            // Arrange
            Flashcard flashcard = new(size);
            float delta = 0.000001f;
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

        [Fact]
        public void LastNAccuracyShouldFailForBadN()
        {
            // Arrange
            Flashcard flashcard = new(10);
            bool[] answers = { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true };

            // Act
            foreach (var answer in answers)
            {
                flashcard.Answer(answer);
            }

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { flashcard.LastNAccuracy(-1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { flashcard.LastNAccuracy(11); });
        }

        [Theory]
        [InlineData(17,
            new bool[] { },
            0, 0)]
        [InlineData(17,
            new bool[] { false },
            0, 0)]
        [InlineData(17,
            new bool[] { true },
            0, 0)]
        [InlineData(17,
            new bool[] { true },
            1, 1)]
        [InlineData(17,
            new bool[] { false, true, false },
            0, 0)]
        [InlineData(17,
            new bool[] { false, true, false },
            1, 0)]
        [InlineData(17,
            new bool[] { false, true, false },
            2, 1.0 / 2.0)]
        [InlineData(17,
            new bool[] { false, true, false },
            3, 1.0 / 3.0)]
        [InlineData(17,
            new bool[] { false, true, false, true, true, false, true, false, false },
            0, 0)]
        [InlineData(17,
            new bool[] { false, true, false, true, true, false, true, false, false },
            3, 1.0 / 3.0)]
        [InlineData(17,
            new bool[] { false, true, false, true, true, false, true, false, false },
            8, 4.0 / 8.0)]
        [InlineData(17,
            new bool[] { false, true, false, true, true, false, true, false, false },
            9, 4.0 / 9.0)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            0, 0)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            1, 1)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            3, 1.0 / 3.0)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            10, 4.0 / 10.0)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            16, 9.0 / 16.0)]
        [InlineData(17,
            new bool[] { false, false, true, true, true, false, true, true, true, true, true, true, false, true, true, true, false, false, true, true, false, false, false, false, true },
            17, 10.0 / 17.0)]
        public void LastNAccuracyShouldWork(int size, bool[] answersToGive, int n, float expectedAccuracy)
        {
            // Arrange
            Flashcard flashcard = new(size);
            float delta = 0.000001f;
            float expectedMin = Math.Max(expectedAccuracy - delta, 0);
            float expectedMax = Math.Min(expectedAccuracy + delta, 1);

            // Act
            foreach (var answer in answersToGive)
            {
                flashcard.Answer(answer);
            }
            float actualAccuracy = flashcard.LastNAccuracy(n);

            // Assert
            Assert.InRange(actualAccuracy, expectedMin, expectedMax);
        }
    }
}
