using System.Collections;
using System.Collections.Generic;

namespace Application.UnitTests.Utilities.SigningUtilities
{
    public class TestData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new()
        {
            new object[] {
                        new Dictionary<string, string>()
                        {
                            { "1", "A" },
                            { "2", "B" },
                            { "3", "C" },
                        },
                        "1:2:3:A:B:C"
                    },
            new object[] {
                        new Dictionary<string, string>()
                        {
                            { "A", "1" },
                            { "B", "2" },
                            { "C", "3" },
                        },
                        "A:B:C:1:2:3"
                    }
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
