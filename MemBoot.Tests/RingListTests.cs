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
            IList<string> ringList = new RingList<string>();
            int expected = 0;

            // Act
            int actual = ringList.Count;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
