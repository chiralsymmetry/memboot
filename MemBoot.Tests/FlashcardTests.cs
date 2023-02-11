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
    }
}
