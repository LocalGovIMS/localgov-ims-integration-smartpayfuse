using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Application.UnitTests.Utilities.SigningUtilities
{
    public class BuildSigningStringTests
    {
        [Theory]
        [ClassData(typeof(TestData))]
        public void BuildSigningString_returns_the_expected_result(Dictionary<string, string> source, string expectedResult)
        {
            // Arrange

            // Act
            var result = Application.SigningUtilities.BuildSigningString(source);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
