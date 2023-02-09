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
    }
}
