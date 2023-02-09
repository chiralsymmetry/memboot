using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemBoot;
using Xunit.Abstractions;

namespace MemBoot.Tests
{
    public class RingListTests
    {
        [Fact]
        public void EmptyListShouldHaveCountZero()
        {
            // Arrange
            IList<string> ringList = new RingList<string>(10);
            int expected = 0;

            // Act
            int actual = ringList.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddingItemShouldGiveCountOne()
        {
            // Arrange
            IList<string> ringList = new RingList<string>(10);
            int expected = 1;

            // Act
            ringList.Add("");
            int actual = ringList.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearingShouldGiveCountZero()
        {
            // Arrange
            IList<string> ringList = new RingList<string>(10);
            int expected = 0;

            // Act
            ringList.Add("");
            ringList.Clear();
            int actual = ringList.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(10, 0, new int[] { })]
        [InlineData(10, 1, new int[] { 0 })]
        [InlineData(10, 5, new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(10, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData(10, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        public void AddingSomeItemsShouldGiveCorrectCount(int capacity, int expectedCount, int[] items)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in items)
            {
                ringList.Add(item);
            }
            int actual = ringList.Count;

            // Assert
            Assert.Equal(expectedCount, actual);
        }

        [Theory]
        [InlineData(10,
            new int[] { 0 },
            new int[] { 0 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        public void AddedItemsShouldBeContained(int capacity, int[] items, int[] expectedItems)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in items)
            {
                ringList.Add(item);
            }

            // Assert
            foreach (var item in expectedItems)
            {
                Assert.True(ringList.Contains(item));
            }
        }

        [Fact]
        public void NotAddedItemsShouldBeNotContained()
        {
            // Arrange
            int[] itemsToAdd = new int[] { 0, 1, 2, 3, 4 };
            int[] itemsNotToAdd = new int[] { 5, 6, 7, 8, 9 };
            IList<int?> ringList = new RingList<int?>(10);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }

            // Assert
            foreach (var item in itemsNotToAdd)
            {
                Assert.False(ringList.Contains(item));
            }
        }
    }
}
