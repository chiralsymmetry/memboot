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
        public void EmptyListShouldNotContainAnything()
        {
            // Arrange
            int[] itemsToAdd = Array.Empty<int>();
            int?[] itemsNotToAdd = new int?[] { 0, 1, 2, 3, 4, null };
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

        [Fact]
        public void NotAddedItemsShouldBeNotContained()
        {
            // Arrange
            int[] itemsToAdd = new int[] { 0, 1, 2, 3, 4 };
            int?[] itemsNotToAdd = new int?[] { 5, 6, 7, 8, 9, null };
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

        [Fact]
        public void OverwrittenItemsShouldBeNotContained()
        {
            // Arrange
            int?[] itemsToAdd = new int?[] { null, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int?[] itemsToOverwrite = new int?[] { null, 0, 1, 2, 3, 4 };
            IList<int?> ringList = new RingList<int?>(5);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }

            // Assert
            foreach (var item in itemsToOverwrite)
            {
                Assert.False(ringList.Contains(item));
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 2, 4, 1, 3, 0 },
            new int[] { 2, 4, 1, 3, 0 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public void IndexOfShouldWork(int capacity, int[] itemsToAdd, int[] itemsToLookFor, int[] expectedIndices)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }

            // Assert
            for (int i = 0; i < itemsToLookFor.Length; i++)
            {
                var itemToLookFor = itemsToLookFor[i];
                var expectedIndex = expectedIndices[i];
                var actualIndex = ringList.IndexOf(itemToLookFor);
                Assert.Equal(expectedIndex, actualIndex);
            }
        }

        [Fact]
        public void RemovingNoncontainedItemsShouldDoNothing()
        {
            // Arrange
            int?[] itemsToAdd = new int?[] { 0, 1, 2, 3, 4 };
            int?[] itemsToRemove = new int?[] { null, 5, 6, 7, 8, 9 };
            IList<int?> ringList = new RingList<int?>(10);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }
            bool actualResult = false;
            foreach (var item in itemsToRemove)
            {
                actualResult |= ringList.Remove(item);
            }
            int expectedCount = itemsToAdd.Length;

            // Assert
            Assert.False(actualResult);
            Assert.Equal(expectedCount, ringList.Count);
            foreach (var item in itemsToAdd)
            {
                Assert.True(ringList.Contains(item));
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 2, 4 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 1, 3 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 5, 6, 7, 8, 9 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 0, 2, 4, 6, 8 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 1, 3, 5, 7, 9 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 3, 4, 5, 6, 7 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 8, 9, 10, 11, 12 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 4, 6, 8, 10, 12 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 3, 5, 7, 9, 11 })]
        public void RemovedItemsShouldNotBeContained(int capacity, int[] itemsToAdd, int[] itemsToRemove)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }
            bool actualResult = true;
            foreach (var item in itemsToRemove)
            {
                actualResult &= ringList.Remove(item);
            }

            // Assert
            Assert.True(actualResult);
            foreach (var item in itemsToRemove)
            {
                Assert.False(ringList.Contains(item));
            }
        }

        [Fact]
        public void RemovingByBadIndexShouldThrowException()
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(10) { 0, 1, 2, 3, 4 };
            int[] badIndices = new int[] { -10, -1, 5, 9, 10 };

            // Act & Assert
            foreach (var i in badIndices)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    ringList.RemoveAt(i);
                });
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { },
            new int[] { })]
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
        public void CopyToShouldWork(int capacity, int[] itemsToAdd, int[] expectedArray)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }
            int?[] actualArray = ringList.ToArray();

            // Assert
            for (int i = 0; i < expectedArray.Length; i++)
            {
                var expectedItem = expectedArray[i];
                var actualItem = actualArray[i];
                Assert.Equal(expectedItem, actualItem);
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { 100 },
            new int[] { 0 },
            new int[] { })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104 },
            new int[] { 0, 1, 2 },
            new int[] { 101, 103 })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104 },
            new int[] { 4, 3, 0 },
            new int[] { 101, 102 })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109 },
            new int[] { 0, 4, 7, 0, 1, 1 },
            new int[] { 102, 106, 107, 108 })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112 },
            new int[] { 0, 4, 7, 0, 1, 1 },
            new int[] { 105, 109, 110, 111 })]
        public void RemoveAtShouldWork(int capacity, int[] itemsToAdd, int[] indicesToRemove, int[] expectedArray)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }
            foreach (var i in indicesToRemove)
            {
                ringList.RemoveAt(i);
            }
            int?[] actualArray = ringList.ToArray();

            // Assert
            for (int i = 0; i < expectedArray.Length; i++)
            {
                var expectedItem = expectedArray[i];
                var actualItem = actualArray[i];
                Assert.Equal(expectedItem, actualItem);
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { },
            new int[] { -10, -1, 1, 9, 10, 11 })]
        [InlineData(10,
            new int[] { 0 },
            new int[] { -10, -1, 2, 9, 10, 11 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { -10, -1, 6, 9, 10, 11 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { -10, -1, 10, 11 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { -10, -1, 10, 11 })]
        public void InsertingAtBadIndexShouldThrowException(int capacity, int[] itemsToAdd, int[] badIndices)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }

            // Assert
            foreach (var i in badIndices)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    ringList.Insert(i, 99);
                });
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { },
            new int[] { 0 },
            new int[] { 200 },
            new int[] { 200 })]
        [InlineData(10,
            new int[] { 100 },
            new int[] { 0 },
            new int[] { 200 },
            new int[] { 200, 100 })]
        [InlineData(10,
            new int[] { 100 },
            new int[] { 1 },
            new int[] { 200 },
            new int[] { 100, 200 })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104 },
            new int[] { 0, 1, 6, 8 },
            new int[] { 200, 201, 202, 203 },
            new int[] { 200, 201, 100, 101, 102, 103, 202, 104, 203 })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109 },
            new int[] { 0, 1, 6, 8, 9 },
            new int[] { 200, 201, 202, 203, 204 },
            new int[] { 200, 201, 100, 101, 102, 103, 202, 104, 203, 204 })]
        [InlineData(10,
            new int[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112 },
            new int[] { 0, 1, 6, 8, 9, 1 },
            new int[] { 200, 201, 202, 203, 204, 205 },
            new int[] { 200, 205, 201, 103, 104, 105, 106, 202, 107, 203 })]
        public void InsertShouldWork(int capacity, int[] itemsToAdd, int[] indicesToInsert, int[] itemsToInsert, int[] expectedArray)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }

            // Act
            for (int i = 0; i < indicesToInsert.Length; i++)
            {
                int indexToInsert = indicesToInsert[i];
                int itemToInsert = itemsToInsert[i];
                ringList.Insert(indexToInsert, itemToInsert);
            }
            int?[] actualArray = ringList.ToArray();

            // Assert
            for (int i = 0; i < expectedArray.Length; i++)
            {
                var expectedItem = expectedArray[i];
                var actualItem = actualArray[i];
                Assert.Equal(expectedItem, actualItem);
            }
        }

        [Fact]
        public void IsReadOnlyShouldBeFalse()
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(1);

            // Assert
            Assert.False(ringList.IsReadOnly);
        }

        [Fact]
        public void BadIndexReadShouldFail()
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(10) { 0, 1, 2, 3, 4 };
            int[] badIndices = new int[] { -1, 5, 9, 10, 11 };

            // Act & Assert
            foreach (var i in badIndices)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ringList[i]);
            }
        }

        [Theory]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 2, 4, 1, 3, 0 },
            new int[] { 2, 4, 1, 3, 0 })]
        [InlineData(10,
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        public void IndexReadShouldWork(int capacity, int[] itemsToAdd, int[] indicesToTry, int[] expectedItems)
        {
            // Arrange
            IList<int?> ringList = new RingList<int?>(capacity);

            // Act
            foreach (var item in itemsToAdd)
            {
                ringList.Add(item);
            }

            // Assert
            for (int i = 0; i < indicesToTry.Length; i++)
            {
                var indexToTry = indicesToTry[i];
                var expectedItem = expectedItems[i];
                var actualItem = ringList[indexToTry];
                Assert.Equal(expectedItem, actualItem);
            }
        }
    }
}
