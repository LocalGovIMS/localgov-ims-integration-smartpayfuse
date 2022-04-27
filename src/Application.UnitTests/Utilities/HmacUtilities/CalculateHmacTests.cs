using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Utilities.HmacUtilities
{
    public class CalculateHmacTests
    {
        [Theory]
        [ClassData(typeof(TestData))]
        public void CalculateHmac_throws_a_HmacGenerationException_when_an_error_occurs(string hmacKey, string signingString, string expectedResult)
        {
            // Arrange
            hmacKey = null;

            // Act
            var result = Assert.Throws<HmacGenerationException>(() => Application.HmacUtilities.CalculateHmac(hmacKey, signingString));

            // Assert
            result.Message.Should().Be("Failed to generate HMAC");
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void CalculateHmac_returns_the_expected_result(string hmacKey, string signingString, string expectedResult)
        {
            // Arrange

            // Act
            var result = Application.HmacUtilities.CalculateHmac(hmacKey, signingString);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
