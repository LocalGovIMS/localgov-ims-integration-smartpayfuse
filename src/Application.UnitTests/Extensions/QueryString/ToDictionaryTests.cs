using Application.Extensions;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Application.UnitTests.Extensions.QueryString
{
    public class ToDictionaryTests
    {
        private readonly Microsoft.AspNetCore.Http.QueryString _queryString;

        public ToDictionaryTests()
        {
            _queryString = new Microsoft.AspNetCore.Http.QueryString("?Item1=Value1&Item2=Value2");
        }

        [Fact]
        public void ToDictionary_returns_a_dictionary()
        {
            // Arrange

            // Act
            var result = _queryString.ToDictionary();

            // Assert
            result.Should().BeOfType<Dictionary<string, string>>();
        }

        [Fact]
        public void ToDictionary_returns_a_dictionary_with_the_expected_number_of_items()
        {
            // Arrange

            // Act
            var result = _queryString.ToDictionary();

            // Assert
            result.Count.Should().Be(2);
        }

        [Fact]
        public void ToDictionary_returns_a_dictionary_with_the_expected_keys()
        {
            // Arrange

            // Act
            var result = _queryString.ToDictionary();

            // Assert
            result.ContainsKey("Item1").Should().BeTrue();
            result.ContainsKey("Item2").Should().BeTrue();
        }
    }
}
